using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;
using Proxoft.DocxToPdf.Documents.Paragraphs.Fields;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class ElementLayoutBuilder
{
    public static ElementLayout CreateElementLayout(this Element element, float xPosition, FieldVariables fieldVariables, LayoutServices services)
    {
        (Size size, float baseLineOffset) = services.CalculateBoundingSizeAndBaseline(element, fieldVariables);

        Rectangle boundingBox = new(new Position(xPosition, 0), size);
        ElementLayout layout = element switch
        {
            Text t => new TextLayout(element.Id, size, baseLineOffset, boundingBox, baseLineOffset, t, Borders.None, LayoutPartition.StartEnd),
            InlineDrawing i => new ImageLayout(element.Id, i.Image, size, baseLineOffset, boundingBox, baseLineOffset, element.TextStyle, Borders.None),
            Space => new SpaceLayout(element.Id, size, baseLineOffset, boundingBox, baseLineOffset, Borders.None, element.TextStyle),
            PageNumberField => new PageNumberLayout(element.Id, fieldVariables.CurrentPage.ToString(), size, baseLineOffset, boundingBox, baseLineOffset, Borders.None, element.TextStyle, LayoutPartition.StartEnd),
            TotalPagesField => new TotalPagesLayout(element.Id, fieldVariables.TotalPages.ToString(), size, baseLineOffset, boundingBox, baseLineOffset, Borders.None, element.TextStyle, LayoutPartition.StartEnd),
            PageBreak => new PageBreakLayout(element.Id, element.TextStyle),
            _ => new EmptyLayout(element.Id, boundingBox, element.TextStyle)
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

    public static ElementLayout Update(this ElementLayout layout, Element[] allElements, FieldVariables fieldVariables, LayoutServices services) =>
        layout
            .Update(allElements.Single(e => e.Id == layout.Id), fieldVariables, services)
            .ResetOffset();

    private static ElementLayout Update(this ElementLayout layout, Element element, FieldVariables fieldVariables, LayoutServices services) =>
        layout is TotalPagesLayout
            ? element.CreateElementLayout(0, fieldVariables, services)
            : layout;
}
