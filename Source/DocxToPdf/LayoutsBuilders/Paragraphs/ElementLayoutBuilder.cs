using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Paragraphs.Fields;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class ElementLayoutBuilder
{
    public static ElementLayout CreateLayout(this Element element, Position onPosition, FieldVariables fieldVariables, LayoutServices services)
    {
        (Size size, float baseLineOffset) = services.CalculateBoundingSizeAndBaseline(element, fieldVariables);

        Rectangle boundingBox = new(onPosition, size);
        ElementLayout layout = element switch
        {
            Text t => new TextLayout(size, baseLineOffset, boundingBox, baseLineOffset, t, Borders.None, LayoutPartition.StartEnd),
            Space => new SpaceLayout(size, baseLineOffset, boundingBox, baseLineOffset, Borders.None, element.TextStyle),
            PageNumberField => new PageNumberLayout(fieldVariables.CurrentPage.ToString(), size, baseLineOffset, boundingBox, baseLineOffset, Borders.None, element.TextStyle, LayoutPartition.StartEnd),
            TotalPagesField => new TotalPagesLayout(fieldVariables.TotalPages.ToString(), size, baseLineOffset, boundingBox, baseLineOffset, Borders.None, element.TextStyle, LayoutPartition.StartEnd),
            _ => new EmptyLayout(boundingBox, element.TextStyle)
        };

        return layout;
    }

    public static ElementLayout UpdateBoudingBox(this ElementLayout layout, float lineHeight, float lineBaseLineOffset)
    {
        Rectangle bb = layout.BoundingBox
            .SetHeight(lineHeight);

        return layout with
        {
            BoundingBox = bb,
            LineBaseLineOffset = lineBaseLineOffset,
        };
    }
}
