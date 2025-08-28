using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal record LayoutingArea(
    Size AvailableSize,
    Rectangle[] Reserved
);

internal record ParagraphLayoutingArea(
    Size AvailableSize,
    float YOffset,
    float LineParagraphYOffset,
    Rectangle[] Reserved)
: LayoutingArea(AvailableSize, Reserved)
{
}

internal static class ParagraphLayoutingAreaOperators
{
    public static ParagraphLayoutingArea ProceedBy(this ParagraphLayoutingArea area, float height) =>
        area with
        {
            YOffset = area.YOffset + height,
            LineParagraphYOffset = area.LineParagraphYOffset + height,
            AvailableSize = area.AvailableSize.DecreaseHeight(height)
        };

    public static Rectangle[] CreateHorizontalSpaces(this ParagraphLayoutingArea area, float expectedLineHeight)
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

        Rectangle[] horizontalSpaces = [];
        float right = 0;

        foreach(Rectangle r in significantReserved)
        {
            if(right < r.X)
            {
                float width = r.X - right;
                Rectangle hs = new(
                    new Position(right, area.YOffset),
                    new Size(width, expectedLineHeight)
                );

                horizontalSpaces = [.. horizontalSpaces, hs];
            }

            if (right < r.Right)
            {
                right = r.Right;
            }

            //float startX = x;
            //float width = r.X - x;

            //x = r.Right;

            //if (width <= 0) // with some precission
            //{

            //    continue;
            //}

            

            //horizontalSpaces = [.. horizontalSpaces, hs];
            //rightX = hs.Right;
        }

        if(right < area.AvailableSize.Width)
        {
            Rectangle remaining = new(
                 new Position(right, area.YOffset),
                 new Size(area.AvailableSize.Width - right, expectedLineHeight)
            );

            horizontalSpaces = [.. horizontalSpaces, remaining];
        }

        return horizontalSpaces;
    }

    private static bool HasOverlapWithLine(this Rectangle rectangle, float yPosition, float expectedLineHeight) =>
        rectangle.Y <= yPosition + expectedLineHeight
        && rectangle.Bottom >= yPosition;
}
