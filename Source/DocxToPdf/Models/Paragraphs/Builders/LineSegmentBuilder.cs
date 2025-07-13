using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements.Fields;
using Proxoft.DocxToPdf.Models.Styles;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Builders;

internal static class LineSegmentBuilder
{
    public static LineSegment CreateLineSegment(
        this Stack<LineElement> fromElements,
        HorizontalSpace space,
        LineAlignment lineAlignment,
        double defaultLineHeight,
        PageVariables variables)
    {
        int overflow = lineAlignment == LineAlignment.Justify ? 2 : 0;
        LineElement[] elements = fromElements
            .GetElementsToFitMaxWidth(space.Width + overflow, variables);

        return new LineSegment(elements, lineAlignment, space, defaultLineHeight);
    }

    private static LineElement[] GetElementsToFitMaxWidth(
        this Stack<LineElement> fromElements,
        double maxWidth,
        PageVariables variables)
    {
        double aggregatedWidth = 0.0;
        List<LineElement> elements = [];
        List<SpaceElement> spaces = [];

        while (fromElements.Count > 0 && aggregatedWidth <= maxWidth)
        {
            LineElement element = fromElements.Pop();
            if(element is Field field)
            {
                field.Update(variables);
            }

            if(element is SpaceElement space)
            {
                spaces.Add(space);
                continue;
            }

            aggregatedWidth += spaces.TotalWidth() + element.Size.Width;
            if (aggregatedWidth < maxWidth)
            {
                elements.AddRange(spaces);
                elements.Add(element);
                spaces.Clear();
            }
            else
            {
                fromElements.Push(element);
            }

            if (element is NewLineElement)
            {
                break;
            }
        }

        return [..elements.Union(spaces)];
    }

    private static double TotalWidth(this IEnumerable<LineElement> elements)
    {
        double w = elements.Sum(e => e.Size.Width);
        return w;
    }
}
