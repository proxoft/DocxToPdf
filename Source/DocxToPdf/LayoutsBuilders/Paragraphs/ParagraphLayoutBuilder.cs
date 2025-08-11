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
    private static readonly float _defaultLineHeight = 12f; // Default line height, can be adjusted based on font size or other factors

    public static ParagraphLayoutingResult Process(
        this Paragraph paragraph,
        LayoutingResult previousLayoutingResult,
        Rectangle availableArea,
        LayoutServices services)
    {
        ParagraphLayoutingResult plr = previousLayoutingResult.AsResultOfModel(paragraph.Id, ParagraphLayoutingResult.None);

        Rectangle ar = plr.StartFromElementId == ModelId.None
            ? availableArea
            : availableArea.CropFromTop(paragraph.Style.ParagraphSpacing.Before);

        return paragraph.ProcessInternal(plr, ar, services);
    }

    public static ParagraphLayoutingResult ProcessInternal(
        this Paragraph paragraph,
        ParagraphLayoutingResult previousLayoutingResult,
        Rectangle availableArea,
        LayoutServices services)
    {
        ModelId continueFrom = previousLayoutingResult.StartFromElementId;

        IEnumerable<Element> unprocessed = paragraph.Elements
            .SkipProcessed(continueFrom);

        (LineLayout[] lines, float spaceAfterLastLine, ModelId lastProcessedElementId, ResultStatus status) = unprocessed.CreateLines(availableArea, paragraph.Style, services);

        Rectangle paragraphBb = lines
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(new Rectangle(availableArea.X, availableArea.Y, 0, 0))
            .CalculateBoundingBox()
            .ExpandHeight(spaceAfterLastLine)
            ;

        Rectangle remainingArea = Rectangle.FromCorners(paragraphBb.BottomLeft, availableArea.BottomRight);
        LayoutPartition partition = status.CalculateLayoutPartition(previousLayoutingResult);
        ParagraphLayout pl = new([.. lines], paragraphBb, Borders.None, partition);

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
            (LineLayout line, ModelId lastElementId) = unprocessed.CreateLine(currentPosition, availableArea.Width, style.TextStyle, services);
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

            keepProcessing = (remainingHeight > 0) && unprocessed.Length > 0;
        } while (keepProcessing);


        ResultStatus status = 
            lines.Count == 0 ? ResultStatus.IgnoreAndRequestDrawingArea
            : unprocessed.Length > 0 ? ResultStatus.RequestDrawingArea
            : ResultStatus.Finished;

        return ([..lines], spaceAfterLastLine, lastProcessed, status);
    }

    private static (LineLayout line, ModelId lastElementId) CreateLine(
        this Element[] elements,
        Position startPosition,
        float availableWidth,
        TextStyle textStyle,
        LayoutServices services)
    {
        float lineWidth = 0;
        ElementLayout[] elementLayouts = [];
        ModelId lastElementId = ModelId.None;
        bool interrupted = false;
        Position currentPosition = startPosition;

        foreach (Element element in elements)
        {
            ElementLayout elementLayout = element.CreateLayout(currentPosition, services);
            if (lineWidth + elementLayout.BoundingBox.Width > availableWidth)
            {
                interrupted = true;
                break;
            }

            lastElementId = element.Id;
            elementLayouts = [..elementLayouts, elementLayout];
            currentPosition = new Position(elementLayout.BoundingBox.Right, currentPosition.Y);
            lineWidth += elementLayout.BoundingBox.Width;
        }

        // TODO: known issue: if element does not fit in line, word wrap must be implemented
        return elementLayouts.Length == 0
            ? (CreateEmptyLine(startPosition, textStyle, services), lastElementId)
            : (elementLayouts.CreateLine(!interrupted, textStyle, services), lastElementId);
    }

    private static LineLayout CreateEmptyLine(Position position, TextStyle textStyle, LayoutServices services)
    {
        Rectangle bb = new(position, new Size(0, _defaultLineHeight));
        ElementLayout specialChar = textStyle.CreateLineCharacter(true, bb.TopRight, services);
        return new LineLayout([], true, bb, Borders.None, specialChar);
    }

    private static LineLayout CreateLine(this ElementLayout[] elements, bool isLast, TextStyle textStyle, LayoutServices services)
    {
        float height = elements
            .Select(e => e.Size.Height)
            .DefaultIfEmpty(_defaultLineHeight)
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

        ElementLayout specialChar = textStyle.CreateLineCharacter(isLast, bb.TopRight, services);
        return new LineLayout(e2, isLast, bb, Borders.None, specialChar);
    }
}

file static class Operations
{
    public static ElementLayout CreateLineCharacter(this TextStyle textStyle, bool isLastLine, Position position, LayoutServices services) =>
        isLastLine
            ? textStyle.CreateParagraphSpecialChar(position, services)
            : new EmptyLayout(new Rectangle(position, Size.Zero), Borders.None, textStyle);

    //public static ElementLayout CreateEndOfLineCharacter(this TextStyle textStyle, bool isLastLine, Position position, LayoutServices services)
    //{
    //    Text endOfLineChar = new(ModelId.None, "⏎", textStyle);
    //    return endOfLineChar.CreateLayout(position, services);
    //}

    private static ElementLayout CreateParagraphSpecialChar(this TextStyle textStyle, Position position, LayoutServices services)
    {
        Text paragraphChar = new(ModelId.None, "¶", textStyle);
        return paragraphChar.CreateLayout(position, services);
    }
}
