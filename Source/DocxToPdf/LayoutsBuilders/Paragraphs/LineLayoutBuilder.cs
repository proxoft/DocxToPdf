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
        ParagraphLayoutingArea area,
        FieldVariables fieldVariables,
        ParagraphStyle style,
        LayoutServices services)
    {
        Element[] unprocessed = [.. elements];
        if(unprocessed.Length == 0)
        {
            LineLayout ll = CreateEmptyLine(0, LineDecoration.Last, style.TextStyle, services);
            if(ll.BoundingBox.Height > area.AvailableSize.Height)
            {
                return ([], ProcessingInfo.IgnoreAndRequestDrawingArea);
            }
            return ([ll], ProcessingInfo.Done);
        }

        LineLayout[] lines = [];

        ParagraphLayoutingArea currentArea = area;
        bool keepProcessing;
        do
        {
            LineLayout? ll = unprocessed.TryCreateLine(currentArea, fieldVariables, style.TextStyle, services);
            if (ll is null)
            {
                break;
            }

            if(currentArea.AvailableSize.Height < ll.BoundingBox.Height)
            {
                break;
            }

            currentArea = currentArea.ProceedBy(ll.BoundingBox.Height);
            lines = [.. lines, ll];
            unprocessed = [.. unprocessed.SkipProcessed(ll.Words.LastOrDefault()?.Id ?? ModelId.None, true)];

            float lineSpaceAfterLine = style.ParagraphSpacing.LineSpacing.CalculateSpaceAfterLine(ll.BoundingBox.Height);
            currentArea = currentArea.ProceedBy(lineSpaceAfterLine);

            keepProcessing = currentArea.AvailableSize.Height > 0
                && unprocessed.Length > 0
                && ll.Decoration == LineDecoration.None;
        } while (keepProcessing);

        ProcessingInfo processingInfo = lines.Length == 0 ? ProcessingInfo.IgnoreAndRequestDrawingArea
            : lines.Last().Decoration == LineDecoration.PageBreak ? ProcessingInfo.NewPageRequired
            : unprocessed.Length > 0 ? ProcessingInfo.RequestDrawingArea
            : ProcessingInfo.Done;

        return (lines, processingInfo);
    }

    public static (LineLayout[] lines, UpdateInfo updateInfo) UpdateLineLayouts(
        this LineLayout[] lines,
        Paragraph paragraph,
        ParagraphLayoutingArea area,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        ParagraphLayoutingArea remainingArea = area;
        LineLayout[] updatedLines = [];
        foreach (LineLayout lineLayout in lines)
        {
            (LineLayout updatedLine, UpdateInfo lineUpdateInfo) = lineLayout.TryUpdateLine(paragraph.Elements, remainingArea, fieldVariables, paragraph.Style.TextStyle, services);

            if (remainingArea.AvailableSize.Height < updatedLine.BoundingBox.Height)
            {
                break;
            }

            updatedLines = [..updatedLines, updatedLine];
            remainingArea = remainingArea.ProceedBy(updatedLine.BoundingBox.Height);

            if(lineUpdateInfo == UpdateInfo.ReconstructRequired)
            {
                break;
            }
        }

        if (lines.LastProcessedElementId() != updatedLines.LastProcessedElementId()) // some lines 
        {
            ModelId lastProcesseId = updatedLines.LastProcessedElementId();
            Element[] unprocessed = [.. paragraph.Elements.SkipProcessed(lastProcesseId, true)];
            (LineLayout[] recreatedLines, ProcessingInfo _) = unprocessed.CreateLineLayouts(remainingArea, fieldVariables, paragraph.Style, services);
            updatedLines = [.. updatedLines, .. recreatedLines];
        }

        UpdateInfo updateInfo = lines.LastProcessedElementId() == updatedLines.LastProcessedElementId()
            ? UpdateInfo.Done
            : UpdateInfo.ReconstructRequired;

        return (updatedLines, updateInfo);
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

    private static ElementLayout CreateLineCharacter(
       this TextStyle textStyle,
       LineDecoration lineDecoration,
       float xPosition,
       LayoutServices services) =>
       lineDecoration switch
       {
           LineDecoration.Last => new Text(ModelId.None, "¶", textStyle).CreateElementLayout(xPosition, FieldVariables.None, services),
           LineDecoration.PageBreak => new Text(ModelId.None, "····Page Break····¶", textStyle.ResizeFont(-3)).CreateElementLayout(xPosition, FieldVariables.None, services),
           LineDecoration.ColumnBreak => new Text(ModelId.None, "····Column Break····¶", textStyle.ResizeFont(-3)).CreateElementLayout(xPosition, FieldVariables.None, services),
           _ => new EmptyLayout(ModelId.None, new Rectangle(new Position(xPosition, 0), Size.Zero), textStyle),
       };

    private static LineDecoration CalculateLineDecoration(this ElementLayout[] elementLayouts, Element[] allElements)
    {
        if (allElements.Length == 0) return LineDecoration.Last;    // empty line
        if (elementLayouts.Length == 0) return LineDecoration.None;
        if (elementLayouts.Last() is BreakLayout pb && pb.BreakType == BreakType.Page) return  LineDecoration.PageBreak;
        if (elementLayouts.Last() is BreakLayout cb && cb.BreakType == BreakType.Column) return  LineDecoration.ColumnBreak;
        if (elementLayouts.Last().Id == allElements.Last().Id) return LineDecoration.Last;
        return LineDecoration.None;
    }

    private static LineLayout? TryCreateLine(
        this Element[] elements,
        ParagraphLayoutingArea area,
        FieldVariables fieldVariables,
        TextStyle paragraphTextStyle,
        LayoutServices services)
    {
        if(elements.Length == 0)
        {
            return null;
        }

        ElementLayout[] elementLayouts = [];
        int elementIndex = 0;
        ElementLayout element = elements[elementIndex].CreateElementLayout(0, fieldVariables, services);

        (int index, float x) activeHs = (0, 0);

        float expectedLineHeight = element.Size.Height;
        Rectangle[] horizontalSpaces = area.CreateHorizontalSpaces(expectedLineHeight);

        while (true)
        {
            // check if element fits into horizontal space
            (bool fits, int inHorizontalSpaceIndex, float onXPosition) = element.FitsIn(horizontalSpaces, activeHs);
            if (!fits)
            {
                break;
            }

            activeHs = (inHorizontalSpaceIndex, onXPosition);

            float realXPosition = horizontalSpaces[activeHs.index].Left + activeHs.x;
            element = element.Offset(new Position(realXPosition, 0));
            elementLayouts = [.. elementLayouts, element];

            activeHs = (activeHs.index, activeHs.x + element.Size.Width);
            elementIndex++;
            if(elementIndex >= elements.Length
                || element is BreakLayout)
            {
                break;
            }

            element = elements[elementIndex].CreateElementLayout(0, fieldVariables, services);
            if(element.Size.Height > expectedLineHeight)
            {
                // restart layouting
                expectedLineHeight = element.Size.Height;
                elementIndex = 0;
                activeHs = (0, 0);
                horizontalSpaces = area.CreateHorizontalSpaces(expectedLineHeight);
                elementLayouts = [];
                element = elements[elementIndex].CreateElementLayout(0, fieldVariables, services);
            }
        }

        LineDecoration lineDecoration = elementLayouts.CalculateLineDecoration(elements);
        LineLayout line = elementLayouts.CreateLine(area.YOffset, lineDecoration, paragraphTextStyle, services);

        return line;
    }

    private static (LineLayout, UpdateInfo) TryUpdateLine(
        this LineLayout line,
        Element[] allElements,
        ParagraphLayoutingArea area,
        FieldVariables fieldVariables,
        TextStyle paragraphTextStyle,
        LayoutServices services)
    {
        if(line.Words.Length == 0)
        {
            return (line, UpdateInfo.Done);
        }

        ElementLayout[] updatedElements = [];

        float expectedLineHeight = line.BoundingBox.Height;
        Rectangle[] horizontalSpaces = area.CreateHorizontalSpaces(expectedLineHeight);
        (int index, float x) activeHs = (0, 0);
        int elementIndex = 0;

        while (true)
        {
            ElementLayout updated = line.Words[elementIndex].Update(
                allElements,
                fieldVariables,
                services
            );

            (bool fits, int inHorizontalSpaceIndex, float onXPosition) = updated.FitsIn(horizontalSpaces, activeHs);
            if (!fits)
            {
                break;
            }

            activeHs = (inHorizontalSpaceIndex, onXPosition);
            float realXPosition = horizontalSpaces[activeHs.index].Left + activeHs.x;
            updatedElements = [.. updatedElements, updated.Offset(new Position(realXPosition, 0))];
            activeHs = (activeHs.index, activeHs.x + updated.Size.Width);
            elementIndex++;
            if (elementIndex >= line.Words.Length)
            {
                break;
            }

            if (updated.Size.Height > expectedLineHeight)
            {
                // restart layouting
                expectedLineHeight = updated.Size.Height;
                elementIndex = 0;
                activeHs = (0, 0);
                horizontalSpaces = area.CreateHorizontalSpaces(expectedLineHeight);
                updatedElements = [];
            }
        }

        LineLayout ll = updatedElements.CreateLine(area.YOffset, line.Decoration, paragraphTextStyle, services);
        UpdateInfo updateInfo = line.Words.Last().Id == ll.Words.Last().Id
            ? UpdateInfo.Done
            : UpdateInfo.ReconstructRequired;
        return (ll, updateInfo);
    }

    private static (bool, int horizontalSpace, float onXPosition) FitsIn(
        this ElementLayout element,
        Rectangle[] spaces,
        (int index, float xPosition) activeHs)
    {
        int i = activeHs.index;
        float x = activeHs.xPosition;

        foreach(Rectangle space in spaces.Skip(activeHs.index))
        {
            if(space.Width >= x + element.Size.Width)
            {
                return (true, i, x);
            }

            i++;
            x = 0;
        }

        return (false, -1, 0);
    }
}