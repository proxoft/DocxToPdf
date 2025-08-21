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

internal static class LineLayoutBuilder
{
    public static (LineLayout[] lines, ProcessingInfo processingInfo) CreateLineLayouts(
        this IEnumerable<Element> elements,
        Size availableArea,
        FieldVariables fieldVariables,
        ParagraphStyle style,
        LayoutServices services)
    {
        Element[] unprocessed = [.. elements];
        LineLayout[] lines = [];
        float currentY = 0;
        float remainingHeight = availableArea.Height;
        bool keepProcessing;

        do
        {
            LineLayout line  = unprocessed.CreateLine(currentY, availableArea.Width, fieldVariables, style.TextStyle, services);
            remainingHeight -= line.BoundingBox.Height;
            float lineSpaceAfterLine = style.ParagraphSpacing.LineSpacing.CalculateSpaceAfterLine(line.BoundingBox.Height);

            if (remainingHeight >= 0)
            {
                lines = [.. lines, line];
                unprocessed = [.. unprocessed.SkipProcessed(line.Words.LastOrDefault()?.Id ?? ModelId.None, true)];
                currentY += line.BoundingBox.Height;
            }

            remainingHeight -= lineSpaceAfterLine;
            currentY += lineSpaceAfterLine;

            keepProcessing = (remainingHeight > 0) && unprocessed.Length > 0
                && line.Decoration == LineDecoration.None;
        } while (keepProcessing);

        ProcessingInfo processingInfo = lines.Length == 0 ? ProcessingInfo.IgnoreAndRequestDrawingArea
            : lines.Last().Decoration == LineDecoration.PageBreak ? ProcessingInfo.NewPageRequired
            : unprocessed.Length > 0 ? ProcessingInfo.RequestDrawingArea
            : ProcessingInfo.Done;

        return (lines, processingInfo);
    }

    public static (LineLayout[] lines, float spaceAfterLastLine, ModelId lastProcessedElementId, ResultStatus status) CreateLines(
        this IEnumerable<Element> elements,
        Rectangle availableArea,
        FieldVariables fieldVariables,
        ParagraphStyle style,
        LayoutServices services)
    {
        Element[] unprocessed = [.. elements];
        bool isEmpty = unprocessed.Length == 0;

        List<LineLayout> lines = [];
        ModelId lastProcessed = ModelId.None;

        bool keepProcessing = true;
        float remainingHeight = availableArea.Height;

        float currentY = availableArea.Y;
        float spaceAfterLastLine = 0;
        do
        {
            LineLayout line = unprocessed.CreateLine(currentY, availableArea.Width, fieldVariables, style.TextStyle, services);
            remainingHeight -= line.BoundingBox.Height;

            float lineSpaceAfterLine = style.ParagraphSpacing.LineSpacing.CalculateSpaceAfterLine(line.BoundingBox.Height);

            if (remainingHeight >= 0)
            {
                lines.Add(line);
                lastProcessed = line.Words.LastOrDefault()?.Id ?? ModelId.None;
                unprocessed = [.. unprocessed.SkipProcessed(lastProcessed, true)];
                currentY += line.BoundingBox.Height;
            }

            remainingHeight -= lineSpaceAfterLine;
            currentY += lineSpaceAfterLine;

            if (unprocessed.Length == 0)
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

        return ([.. lines], spaceAfterLastLine, lastProcessed, status);
    }

    public static (LineLayout[] lines, ProcessingInfo) UpdateLineLayouts(
        this LineLayout[] lines,
        Paragraph paragraph,
        Size availableArea,
        FieldVariables fieldVariables,
        ParagraphStyle style,
        LayoutServices services)
    {
        LineLayout[] updatedLines = [];
        float currentY = 0;
        float remainingHeight = availableArea.Height;

        ProcessingInfo processingInfo = ProcessingInfo.Done;

        foreach (LineLayout line in lines)
        {
            (LineLayout updatedLine, processingInfo) = line.Update(
                paragraph.Elements,
                currentY,
                availableArea.Width,
                fieldVariables,
                style.TextStyle,
                services
            );

            if(remainingHeight - updatedLine.BoundingBox.Height < 0)
            {
                processingInfo = ProcessingInfo.RequestDrawingArea;
                break;
            }

            remainingHeight -= updatedLine.BoundingBox.Height;
            currentY += updatedLine.BoundingBox.Height;

            updatedLines = [.. updatedLines, updatedLine];
            if(processingInfo == ProcessingInfo.ReconstructRequired)
            {
                break;
            }
        }

        if (processingInfo == ProcessingInfo.ReconstructRequired)
        {
            ModelId lastProcesseId = updatedLines.LastProcessedElement();
            Element[] unprocessed = [.. paragraph.Elements.SkipProcessed(lastProcesseId, true)];

            (LineLayout[] reconstructedLines, processingInfo) = unprocessed.CreateLineLayouts(new Size(availableArea.Width, remainingHeight), fieldVariables, paragraph.Style, services);
            updatedLines = [.. updatedLines, .. reconstructedLines];
        }

        return (updatedLines, processingInfo);
    }

    private static (LineLayout, ProcessingInfo) Update(
        this LineLayout line,
        Element[] elements,
        float yPosition,
        float availableWidth,
        FieldVariables fieldVariables,
        TextStyle textStyle,
        LayoutServices services)
    {
        if (!line.ContainsUpdatableField())
        {
            LineLayout l = line with
            {
                BoundingBox = line.BoundingBox.MoveTo(new Position(line.BoundingBox.Left, yPosition))
            };

            return(l, ProcessingInfo.Done);
        }

        LineLayout updatedLine = line.UpdateLine(elements, yPosition, availableWidth, fieldVariables, textStyle, services);
        ProcessingInfo processingInfo = line.Words.Last().Id == updatedLine.Words.Last().Id
            ? ProcessingInfo.Done
            : ProcessingInfo.ReconstructRequired;

        return (updatedLine, processingInfo);
    }

    private static LineLayout CreateLine(
        this Element[] elements,
        float yPosition,
        float availableWidth,
        FieldVariables fieldVariables,
        TextStyle textStyle,
        LayoutServices services)
    {
        float lineWidth = 0;
        ElementLayout[] elementLayouts = [];
        float xPosition = 0;

        foreach (Element element in elements)
        {
            if (element is PageBreak)
            {
                elementLayouts = [.. elementLayouts, new PageBreakLayout(element.Id, textStyle)];
                break;
            }

            ElementLayout elementLayout = element.CreateElementLayout(xPosition, fieldVariables, services);
            if (lineWidth + elementLayout.BoundingBox.Width > availableWidth)
            {
                break;
            }

            elementLayouts = [.. elementLayouts, elementLayout];
            xPosition = elementLayout.BoundingBox.Right;
            lineWidth += elementLayout.BoundingBox.Width;
        }

        LineDecoration lineDecoration = elementLayouts.CalculateLineDecoration(elements);
        // TODO: known issue: if element does not fit in line, word wrap must be implemented
        LineLayout ll = elementLayouts.CreateLine(yPosition, lineDecoration, textStyle, services);
        return ll;
    }

    private static LineLayout CreateLine(this ElementLayout[] elements, float yPosition, LineDecoration lineDecoration, TextStyle textStyle, LayoutServices services)
    {
        if(elements.Length == 0)
        {
            return CreateEmptyLine(yPosition, lineDecoration, textStyle, services);
        }

        float height = elements
            .Select(e => e.Size.Height)
            .Max();

        Rectangle boundingBox = elements.CalculateBoundingBox();

        float lineBaselineOffset = elements
            .Select(e => e.BaselineOffset)
            .Max();

        ElementLayout[] justifiedElements = [
            ..elements.Select(e => e.UpdateBoudingBox(height, lineBaselineOffset))
        ];

        ElementLayout specialChar = textStyle.CreateLineCharacter(lineDecoration, boundingBox.TopRight.X, services);
        return new LineLayout(justifiedElements, lineDecoration, boundingBox.MoveYBy(yPosition), Borders.None, specialChar);
    }

    private static LineLayout CreateEmptyLine(float YPosition, LineDecoration lineDecoration, TextStyle textStyle, LayoutServices services)
    {
        float defaultLineHeight = services.CalculateLineHeight(textStyle);
        Rectangle bb = new(new Position(0, YPosition), new Size(0, defaultLineHeight));
        ElementLayout specialChar = textStyle.CreateLineCharacter(lineDecoration, 0, services);
        return new LineLayout([], lineDecoration, bb, Borders.None, specialChar);
    }

    private static LineLayout UpdateLine(
        this LineLayout line,
        Element[] allElements,
        float yPosition,
        float availableWidth,
        FieldVariables fieldVariables,
        TextStyle textStyle,
        LayoutServices services)
    {
        if (!line.Words.Any())
        {
            return line;
        }

        ElementLayout[] updatedWords = [];
        float currentLineWidth = 0;

        foreach(ElementLayout old in line.Words)
        {
            Element element = allElements.Single(e => e.Id == old.Id);
            ElementLayout newLayout = old.Update(element, currentLineWidth, fieldVariables, services);
            if (currentLineWidth + newLayout.BoundingBox.Width > availableWidth)
            {
                break;
            }

            currentLineWidth += newLayout.BoundingBox.Width;
            updatedWords = [.. updatedWords, newLayout];
        }

        float height = updatedWords
            .Select(e => e.Size.Height)
            .Max();

        Rectangle bb = updatedWords
            .Select(e => e.BoundingBox)
            .CalculateBoundingBox()
            ;

        float lineBaselineOffset = updatedWords
            .Select(e => e.BaselineOffset)
            .DefaultIfEmpty(0)
            .Max();

        ElementLayout[] e2 = [
             ..updatedWords
            .Select(e => e.UpdateBoudingBox(height, lineBaselineOffset))
        ];

        LineDecoration lineDecoration = e2.CalculateLineDecoration(allElements);
        ElementLayout specialChar = textStyle.CreateLineCharacter(lineDecoration, bb.TopRight.X, services);
        return new LineLayout(
            e2,
            line.Decoration,
            bb,
            line.Borders,
            specialChar
        );
    }

    private static ElementLayout CreateLineCharacter(
       this TextStyle textStyle,
       LineDecoration lineDecoration,
       float xPosition,
       LayoutServices services) =>
       lineDecoration switch
       {
           LineDecoration.Last => new Text(ModelId.None, "¶", textStyle).CreateElementLayout(xPosition, FieldVariables.None, services),
           LineDecoration.PageBreak => new Text(ModelId.None, "····Page Break····¶", textStyle.ResizeFont(-3)).CreateElementLayout(xPosition, FieldVariables.None, services),
           _ => new EmptyLayout(ModelId.None, new Rectangle(new Position(xPosition, 0), Size.Zero), textStyle),
       };


    private static LineDecoration CalculateLineDecoration(this ElementLayout[] elementLayouts, Element[] allElements)
    {
        if (allElements.Length == 0) return LineDecoration.Last;    // empty line
        if (elementLayouts.Length == 0) return LineDecoration.None;
        if (elementLayouts.Last() is PageBreakLayout) return LineDecoration.PageBreak;
        if (elementLayouts.Last().Id == allElements.Last().Id) return LineDecoration.Last;
        return LineDecoration.None;
    }
}
