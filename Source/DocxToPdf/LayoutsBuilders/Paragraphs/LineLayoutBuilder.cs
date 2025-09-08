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
        if (unprocessed.Length == 0)
        {
            LineLayout ll = CreateEmptyLine(0, LineDecoration.Last, style.TextStyle, services);
            if (ll.BoundingBox.Height > area.AvailableSize.Height)
            {
                return ([], ProcessingInfo.IgnoreAndRequestDrawingArea);
            }
            return ([ll], ProcessingInfo.Done);
        }

        LineLayout[] lines = [];

        ParagraphLayoutingArea currentArea = area;
        bool keepProcessing = unprocessed.Length > 0;

        while (keepProcessing) {
            LineLayout ll = unprocessed.CreateLine(
                currentArea,
                style.LineAlignment,
                fieldVariables,
                style.TextStyle,
                services
            );

            if (currentArea.AvailableSize.Height < ll.BoundingBox.Height)
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
        }

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
            (LineLayout updatedLine, UpdateInfo lineUpdateInfo) = lineLayout.TryUpdateLine(
                paragraph.Elements,
                remainingArea,
                fieldVariables,
                paragraph.Style.LineAlignment,
                paragraph.Style.TextStyle,
                services);

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

        Rectangle boundingBox = elements
            .Append(FakeLayout.New(height)) // ensure the line will start on position 0,0
            .CalculateBoundingBox()
            ;

        float lineBaselineOffset = elements
            .Select(e => e.BaselineOffset)
            .Max();

        ElementLayout[] justifiedElements = [
            ..elements.Select(e => e.UpdateBoudingBox(height, lineBaselineOffset))
        ];

        ElementLayout specialChar = textStyle.CreateLineCharacter(lineDecoration, services)
            .Offset(new Position(boundingBox.Right, 0));

        return new LineLayout(justifiedElements, lineDecoration, boundingBox.MoveYBy(yPosition), Borders.None, specialChar);
    }

    private static LineLayout CreateEmptyLine(float YPosition, LineDecoration lineDecoration, TextStyle textStyle, LayoutServices services)
    {
        float defaultLineHeight = services.CalculateLineHeight(textStyle);
        Rectangle bb = new(new Position(0, YPosition), new Size(0, defaultLineHeight));
        ElementLayout specialChar = textStyle.CreateLineCharacter(lineDecoration, services);
        return new LineLayout([], lineDecoration, bb, Borders.None, specialChar);
    }

    private static ElementLayout CreateLineCharacter(
       this TextStyle textStyle,
       LineDecoration lineDecoration,
       LayoutServices services) =>
       lineDecoration switch
       {
           LineDecoration.Last => new Text(ModelId.None, "¶", textStyle).CreateElementLayout(FieldVariables.None, services),
           LineDecoration.PageBreak => new Text(ModelId.None, "····Page Break····¶", textStyle.ResizeFont(-3)).CreateElementLayout(FieldVariables.None, services),
           LineDecoration.ColumnBreak => new Text(ModelId.None, "····Column Break····¶", textStyle.ResizeFont(-3)).CreateElementLayout(FieldVariables.None, services),
           _ => new EmptyLayout(ModelId.None, Rectangle.Empty, textStyle),
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

    private static LineLayout CreateLine(
        this Element[] elements,
        ParagraphLayoutingArea area,
        LineAlignment alignment,
        FieldVariables fieldVariables,
        TextStyle paragraphTextStyle,
        LayoutServices services)
    {
        int elementIndex = 0;
        ElementLayout element = elements[elementIndex].CreateElementLayout(fieldVariables, services);

        float expectedLineHeight = element.Size.Height;
        LineSegment[] lineSegments = area.CreateLineSegments(expectedLineHeight);

        while (true)
        {
            (lineSegments, bool success) = lineSegments.TryAdd(element);

            if (!success
                || elementIndex >= elements.Length - 1
                || element is BreakLayout)
            {
                break;
            }

            elementIndex++;
            if (element.Size.Height > expectedLineHeight)
            {
                // restart line creating
                expectedLineHeight = element.Size.Height;
                lineSegments = area.CreateLineSegments(expectedLineHeight);
                elementIndex = 0;
            }

            element = elements[elementIndex].CreateElementLayout(fieldVariables, services);
        }

        bool isLastLine = lineSegments.IsLastLine(elements);
        ElementLayout[] elementLayouts = [
            ..lineSegments
                .AlignElements(alignment, isLastLine)
                .PositionElementsInLine()
        ];

        LineDecoration lineDecoration = elementLayouts.CalculateLineDecoration(elements);
        LineLayout line = elementLayouts.CreateLine(area.YOffset, lineDecoration, paragraphTextStyle, services);

        return line;
    }

    private static (LineLayout, UpdateInfo) TryUpdateLine(
        this LineLayout line,
        Element[] allElements,
        ParagraphLayoutingArea area,
        FieldVariables fieldVariables,
        LineAlignment alignment,
        TextStyle paragraphTextStyle,
        LayoutServices services)
    {
        if(line.Words.Length == 0)
        {
            return (line, UpdateInfo.Done);
        }

        float expectedLineHeight = line.BoundingBox.Height;
        LineSegment[] lineSegments = area.CreateLineSegments(expectedLineHeight);
        int elementIndex = 0;

        while (true)
        {
            ElementLayout updated = line.Words[elementIndex].Update(
                allElements,
                fieldVariables,
                services
            );

            (lineSegments, bool success) = lineSegments.TryAdd(updated);

            if (!success
                || elementIndex >= line.Words.Length - 1)
            {
                break;
            }

            elementIndex++;
            if (updated.Size.Height > expectedLineHeight)
            {
                // restart
                expectedLineHeight = updated.Size.Height;
                lineSegments = area.CreateLineSegments(expectedLineHeight);
                elementIndex = 0;
            }
        }

        bool isLastLine = lineSegments.IsLastLine(allElements);
        ElementLayout[] updatedElements = [
            ..lineSegments
                .AlignElements(alignment, isLastLine)
                .PositionElementsInLine()
        ];

        LineDecoration lineDecoration = updatedElements.CalculateLineDecoration(allElements);
        LineLayout ll = updatedElements.CreateLine(area.YOffset, lineDecoration, paragraphTextStyle, services);

        UpdateInfo updateInfo = line.Words.Last().Id == ll.Words.Last().Id
            ? UpdateInfo.Done
            : UpdateInfo.ReconstructRequired;

        return (ll, updateInfo);
    }
}

file record LineSegment(
    Rectangle Area,
    bool Active,
    ElementLayout[] Elements,
    Rectangle RemainingArea)
{
    public static LineSegment New(Rectangle area, bool active) =>
        new(area, active, [], new Rectangle(Position.Zero, area.Size));
}

file static class LineSegmentFunctions
{
    public static (LineSegment[] segments, bool success) TryAdd(this LineSegment[] segments, ElementLayout element)
    {
        LineSegment[] updated = [..segments.TakeWhile(s => !s.Active)];
        bool success = false;
        foreach(LineSegment segment in segments.SkipWhile(s => !s.Active))
        {
            (LineSegment updatedSegment, success) = segment.TryAdd(element);
            updated = [
                    ..updated,
                    updatedSegment
            ];

            if (success)
            {
                updated = [
                    ..updated,
                    ..segments.SkipWhile(s => s != segment).Skip(1)
                ];

                break;
            }
        }

        return (updated, success);
    }

    public static LineSegment[] AlignElements(this LineSegment[] segments, LineAlignment lineAlignment, bool isLastLine)
    {
        LineSegment[] alignedSegments = lineAlignment switch
        {
            LineAlignment.Left => segments,
            LineAlignment.Center => [.. segments.Select(s => s.CenterAlignElements())],
            LineAlignment.Right => [..segments.Select(s => s.RightAlignElements())],
            LineAlignment.Justify when isLastLine => segments, // left align
            LineAlignment.Justify => [..segments.Select(s => s.JustifyElements())],
            _ => segments
        };

        return alignedSegments;
    }

    public static IEnumerable<ElementLayout> PositionElementsInLine(this LineSegment[] segments) =>
        segments.SelectMany(s => s.PositionElementsInLine());

    public static bool IsLastLine(this LineSegment[] lineSegments, Element[] allElements)
    {
        if (allElements.Length == 0) return true;

        ElementLayout? last = lineSegments
            .Reverse()
            .SelectMany(s => s.Elements.Reverse())
            .FirstOrDefault();

        if (last is null) return false;
        if (last is BreakLayout) return true;

        return last.Id == allElements[^1].Id;
    }

    private static (LineSegment segment, bool success) TryAdd(this LineSegment segment, ElementLayout element)
    {
        if (!element.FitsIn(segment))
        {
            return (
                segment with
                {
                    Active = false
                },
                false
            );
        }

        LineSegment updated = segment with
        {
            Active = true,
            Elements = [.. segment.Elements, element.ResetOffset().Offset(new Position(segment.RemainingArea.Left, 0))],
            RemainingArea = segment.RemainingArea.CropFromLeft(element.Size.Width)
        };

        return (updated, true);
    }

    private static bool FitsIn(this ElementLayout element, LineSegment segment) =>
        segment.RemainingArea.Width >= element.Size.Width;

    private static LineSegment CenterAlignElements(this LineSegment segment)
    {
        if (segment.Elements.Length == 0) return segment;
        float xDiff = (segment.Area.Size.Width - segment.Elements[^1].BoundingBox.Right) / 2;
        ElementLayout[] alignedElements = [
            ..segment.Elements
                .Select(e => e.Offset(new Position(xDiff, 0)))
        ];

        return segment with
        {
            Elements = alignedElements
        };
    }

    private static LineSegment RightAlignElements(this LineSegment segment)
    {
        if(segment.Elements.Length == 0) return segment;
        float xDiff = segment.Area.Size.Width - segment.Elements[^1].BoundingBox.Right;
        ElementLayout[] alignedElements = [
            ..segment.Elements
                .Select(e => e.Offset(new Position(xDiff, 0)))
        ];

        return segment with
        {
            Elements = alignedElements
        };
    }

    private static LineSegment JustifyElements(this LineSegment segment)
    {
        if (segment.Elements.Length == 0) return segment;
        float availableSpace = segment.Area.Size.Width - segment.Elements[^1].BoundingBox.Right;
        int spacesCount = segment.Elements
            .OfType<SpaceLayout>()
            .Count();

        if(spacesCount == 0) return segment;
        float widthPerSpace = availableSpace / spacesCount;
        float x = 0;
        ElementLayout[] justified = [..segment.Elements
            .Select(e =>
            {
                ElementLayout jel = e.JustifyWidth(widthPerSpace)
                    .ResetOffset()
                    .Offset(new Position(x, 0));

                x += jel.BoundingBox.Width;
                return jel;
            })
        ];

        return segment with
        {
            Elements = justified
        };
    }

    private static ElementLayout JustifyWidth(this ElementLayout elementLayout, float spaceWidth) =>
        elementLayout is SpaceLayout sl
            ? sl with
            {
                BoundingBox = sl.BoundingBox
                            .SetWidth(elementLayout.Size.Width + spaceWidth)
            }
            : elementLayout;

    private static IEnumerable<ElementLayout> PositionElementsInLine(this LineSegment segment) =>
        segment.Elements.Select(e => e.Offset(new Position(segment.Area.Left, 0)));
}

file record FakeLayout(Size Size)
    : ElementLayout(ModelId.None, Size, 0, new Rectangle(new Position(0, 0), Size), 0, Borders.None, LayoutPartition.StartEnd)
{
    public override TextStyle GetTextStyle() => TextStyle.Default;

    public static FakeLayout New(float height) =>
        new(new Size(0, height));
}

file static class ParagraphLayoutingAreaOperators
{
    public static LineSegment[] CreateLineSegments(this ParagraphLayoutingArea area, float expectedLineHeight)
    {
        Rectangle lineArea = new(
            new Position(0, area.LineParagraphYOffset),
            new Size(area.AvailableSize.Width, expectedLineHeight)
        );

        Rectangle[] significantReserved = [
            ..area.Reserved
                .Where(r => r.HasOverlapWithLine(area.LineParagraphYOffset, expectedLineHeight)) // ignore those which dont overlap with line
                .Where(r => r.Right >= 0)                                                        // ignore those which are left from paragraph
                .Where(r => r.Left <= area.AvailableSize.Width)                                  // ignore those which are to the right of paragraph
                .OrderBy(r => r.X)
                .ThenBy(r => r.Right)
        ];

        LineSegment[] lineSegments = [];
        bool isFirst = true;

        // Rectangle[] horizontalSpaces = [];
        float right = 0;

        foreach (Rectangle r in significantReserved)
        {
            if (right < r.X)
            {
                float width = r.X - right;
                Rectangle horizontalSpace = new(
                    new Position(right, area.YOffset),
                    new Size(width, expectedLineHeight)
                );

                lineSegments = [
                    ..lineSegments,
                    LineSegment.New(horizontalSpace, isFirst)
                ];

                isFirst = false;
            }

            if (right < r.Right)
            {
                right = r.Right;
            }
        }

        if (right < area.AvailableSize.Width)
        {
            Rectangle remaining = new(
                 new Position(right, area.YOffset),
                 new Size(area.AvailableSize.Width - right, expectedLineHeight)
            );

            lineSegments = [
                ..lineSegments,
                LineSegment.New(remaining, isFirst)
            ];
        }

        return lineSegments;
    }

    private static bool HasOverlapWithLine(this Rectangle rectangle, float yPosition, float expectedLineHeight) =>
        rectangle.Y <= yPosition + expectedLineHeight
        && rectangle.Bottom >= yPosition;
}