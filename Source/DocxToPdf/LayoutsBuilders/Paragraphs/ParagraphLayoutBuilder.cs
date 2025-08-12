using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class ParagraphLayoutBuilder
{
    public static ParagraphLayoutingResult Process(
        this Paragraph paragraph,
        LayoutingResult previousLayoutingResult,
        Rectangle availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        ParagraphLayoutingResult plr = previousLayoutingResult.AsResultOfModel(paragraph.Id, ParagraphLayoutingResult.None);
        if(plr.Status == ResultStatus.NewPageRequired)
        {
            return new ParagraphLayoutingResult(paragraph.Id, ParagraphLayout.Empty, ModelId.None, availableArea, ResultStatus.Finished);
        }

        Rectangle ar = plr.LastProcessedModelId == ModelId.None
            ? availableArea
            : availableArea.CropFromTop(paragraph.Style.ParagraphSpacing.Before);

        return paragraph.ProcessInternal(plr, ar, fieldVariables, services);
    }

    public static ParagraphLayoutingResult ProcessInternal(
        this Paragraph paragraph,
        ParagraphLayoutingResult previousLayoutingResult,
        Rectangle availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        IEnumerable<Element> unprocessed = paragraph.Elements
            .SkipProcessed(previousLayoutingResult.LastProcessedModelId, finished: true);

        (LineLayout[] lines, float spaceAfterLastLine, ModelId lastProcessedElementId, ResultStatus status) = unprocessed.CreateLines(availableArea, fieldVariables, paragraph.Style, services);

        Rectangle paragraphBb = lines
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(new Rectangle(availableArea.X, availableArea.Y, 0, 0))
            .CalculateBoundingBox()
            .ExpandHeight(spaceAfterLastLine)
            ;

        Rectangle remainingArea = Rectangle.FromCorners(paragraphBb.BottomLeft, availableArea.BottomRight);
        LayoutPartition partition = status.CalculateLayoutPartition(previousLayoutingResult);
        ParagraphLayout pl = new(paragraph.Id, [.. lines], paragraphBb, Borders.None, partition);

        if(status == ResultStatus.Finished)
        {
            remainingArea = remainingArea.CropFromTop(paragraph.Style.ParagraphSpacing.After);
        }

        return new ParagraphLayoutingResult(
            paragraph.Id,
            pl,
            lastProcessedElementId,
            remainingArea,
            status
        );
    }

    private static (LineLayout[] lines, float spaceAfterLastLine, ModelId lastProcessedElementId, ResultStatus status) CreateLines(
        this IEnumerable<Element> elements,
        Rectangle availableArea,
        FieldVariables fieldVariables,
        ParagraphStyle style,
        LayoutServices services)
    {
        Element[] unprocessed = [..elements];
        bool isEmpty = unprocessed.Length == 0;

        List<LineLayout> lines = [];
        ModelId lastProcessed = ModelId.None;

        bool keepProcessing = true;
        float remainingHeight = availableArea.Height;

        Position currentPosition = availableArea.TopLeft;
        float spaceAfterLastLine = 0;
        do
        {
            (LineLayout line, ModelId lastElementId) = unprocessed.CreateLine(currentPosition, availableArea.Width, fieldVariables, style.TextStyle, services);
            remainingHeight -= line.BoundingBox.Height;

            float lineSpaceAfterLine = style.ParagraphSpacing.LineSpacing.CalculateSpaceAfterLine(line.BoundingBox.Height);

            if (remainingHeight >= 0)
            {
                lines.Add(line);
                lastProcessed = lastElementId;
                unprocessed = [..unprocessed.SkipWhile(e => e.Id != lastElementId).Skip(1)];
                currentPosition = currentPosition.ShiftY(line.BoundingBox.Height);
            }

            remainingHeight -= lineSpaceAfterLine;
            currentPosition = currentPosition.ShiftY(lineSpaceAfterLine);

            if(unprocessed.Length == 0)
            {
                spaceAfterLastLine = lineSpaceAfterLine;
            }

            keepProcessing = (remainingHeight > 0) && unprocessed.Length > 0
                && line.Decoration == LineDecoration.None;
        } while (keepProcessing);


        ResultStatus status = 
            lines.Count == 0 ? ResultStatus.IgnoreAndRequestDrawingArea
            : lines.Last().Decoration == LineDecoration.PageBreak ? ResultStatus.NewPageRequired
            : unprocessed.Length > 0 ? ResultStatus.RequestDrawingArea
            : ResultStatus.Finished;

        return ([..lines], spaceAfterLastLine, lastProcessed, status);
    }

    private static (LineLayout line, ModelId lastElementId) CreateLine(
        this Element[] elements,
        Position startPosition,
        float availableWidth,
        FieldVariables fieldVariables,
        TextStyle textStyle,
        LayoutServices services)
    {
        float lineWidth = 0;
        ElementLayout[] elementLayouts = [];
        ModelId lastElementId = ModelId.None;
        Position currentPosition = startPosition;
        bool isPageBreak = false;

        foreach (Element element in elements)
        {
            if (element is PageBreak)
            {
                isPageBreak = true;
                lastElementId = element.Id;
                break;
            }

            ElementLayout elementLayout = element.CreateLayout(currentPosition, fieldVariables, services);
            if (lineWidth + elementLayout.BoundingBox.Width > availableWidth)
            {
                break;
            }

            lastElementId = element.Id;
            elementLayouts = [..elementLayouts, elementLayout];
            currentPosition = new Position(elementLayout.BoundingBox.Right, currentPosition.Y);
            lineWidth += elementLayout.BoundingBox.Width;
        }

        bool isLastLine = elements.Length == 0
            || lastElementId == elements.Last().Id
            ;

        LineDecoration lineDecoration = isPageBreak
            ? LineDecoration.PageBreak
            : isLastLine ? LineDecoration.Last
            : LineDecoration.None;

        // TODO: known issue: if element does not fit in line, word wrap must be implemented
        return elementLayouts.Length == 0
            ? (CreateEmptyLine(startPosition, lineDecoration, textStyle, services), lastElementId)
            : (elementLayouts.CreateLine(lineDecoration, textStyle, services), lastElementId);
    }

    private static LineLayout CreateEmptyLine(Position position, LineDecoration lineDecoration, TextStyle textStyle, LayoutServices services)
    {
        float defaultLineHeight = services.CalculateLineHeight(textStyle);
        Rectangle bb = new(position, new Size(0, defaultLineHeight));
        ElementLayout specialChar = textStyle.CreateLineCharacter(lineDecoration, bb.TopRight, services);
        return new LineLayout([], lineDecoration, bb, Borders.None, specialChar);
    }

    private static LineLayout CreateLine(this ElementLayout[] elements, LineDecoration lineDecoration, TextStyle textStyle, LayoutServices services)
    {
        float defaultLineHeight = services.CalculateLineHeight(textStyle);
        float height = elements
            .Select(e => e.Size.Height)
            .DefaultIfEmpty(defaultLineHeight)
            .Max();

        Rectangle bb = elements.Select(e => e.BoundingBox).CalculateBoundingBox();

        float lineBaselineOffset = elements
            .Select(e => e.BaselineOffset)
            .DefaultIfEmpty(0)
            .Max();

        ElementLayout[] e2 = [
            ..elements
            .Select(e => e.UpdateBoudingBox(height, lineBaselineOffset))
        ];

        ElementLayout specialChar = textStyle.CreateLineCharacter(lineDecoration, bb.TopRight, services);
        return new LineLayout(e2, lineDecoration, bb, Borders.None, specialChar);
    }
}

file static class Operations
{
    public static ElementLayout CreateLineCharacter(
        this TextStyle textStyle,
        LineDecoration lineDecoration,
        Position position,
        LayoutServices services) =>
        lineDecoration switch
        {
            LineDecoration.Last => new Text(ModelId.None, "¶", textStyle).CreateLayout(position, FieldVariables.None, services),
            LineDecoration.PageBreak => new Text(ModelId.None, "····Page Break····¶", textStyle.ResizeFont(-3)).CreateLayout(position, FieldVariables.None, services),
            _ => new EmptyLayout(new Rectangle(position, Size.Zero), textStyle),
        };

    //public static ElementLayout CreateEndOfLineCharacter(this TextStyle textStyle, bool isLastLine, Position position, LayoutServices services)
    //{
    //    Text endOfLineChar = new(ModelId.None, "⏎", textStyle);
    //    return endOfLineChar.CreateLayout(position, services);
    //}
}
