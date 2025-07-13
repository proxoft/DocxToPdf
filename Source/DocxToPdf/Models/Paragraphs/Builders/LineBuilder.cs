using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements.Drawings;
using Proxoft.DocxToPdf.Models.Styles;

using C = Proxoft.DocxToPdf.Core.Structs;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Builders;

internal static class LineBuilder
{
    public static Line CreateLine(
        this Stack<LineElement> fromElements,
        LineAlignment lineAlignment,
        double relativeYOffset,
        IEnumerable<FixedDrawing> fixedDrawings,
        double availableWidth,
        double defaultLineHeight,
        PageVariables variables,
        LineSpacing lineSpacing)
    {
        var (lineSegments, lineHeight) = fromElements
            .CreateLineSegments(lineAlignment, relativeYOffset, fixedDrawings.Select(d => d.BoundingBox), availableWidth, defaultLineHeight, variables);

        var baseLineOffset = lineSegments.Max(ls => ls.GetBaseLineOffset());
        foreach (var ls in lineSegments)
        {
            ls.JustifyElements(baseLineOffset, lineHeight);
        }

        return new Line(lineSegments, lineSpacing);
    }

    private static (LineSegment[], double) CreateLineSegments(
        this Stack<LineElement> fromElements,
        LineAlignment lineAlignment,
        double relativeYOffset,
        IEnumerable<C.Rectangle> fixedDrawings,
        double availableWidth,
        double defaultLineHeight,
        PageVariables variables)
    {
        LineReservedSpaceHelper reserveSpaceHelper = new (fixedDrawings, relativeYOffset, availableWidth);

        double expectedLineHeight = 0.0;
        bool finished = false;

        do
        {
            HorizontalSpace[] segmentSpaces = reserveSpaceHelper.GetLineSegments();

            var lineSegments = segmentSpaces
                .Select((space, i) => fromElements.CreateLineSegment(space, lineAlignment, defaultLineHeight, variables))
                .ToArray();

            var maxHeight = lineSegments.Max(l => l.Size.Height);
            expectedLineHeight = Math.Max(maxHeight, expectedLineHeight);
            var hasChanged = reserveSpaceHelper.UpdateLineHeight(expectedLineHeight);

            if (!hasChanged)
            {
                return (lineSegments, expectedLineHeight);
            }
            else
            {
                foreach (var ls in lineSegments.Reverse())
                {
                    ls.ReturnElementsToStack(fromElements);
                }
            }
        } while (!finished);

        return ([], expectedLineHeight);
    }
}
