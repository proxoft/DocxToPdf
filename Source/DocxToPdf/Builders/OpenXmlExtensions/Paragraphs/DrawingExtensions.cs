using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using WDrawing = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;

internal static class DrawingExtensions
{
    public static InlineDrawing[] CreateInlineDrawing(
        this Word.Drawing drawing,
        TextStyle textStyle,
        BuilderServices builderServices)
    {
        if (drawing.Inline == null)
        {
            return [];
        }

        Size size = drawing.Inline.Extent.ToSize();
        OpenXml.Drawing.Blip blipElement = drawing.Inline.Descendants<OpenXml.Drawing.Blip>().First();
        byte[] image = builderServices.ImageAccessor.GetImageBytes(blipElement.Embed?.Value ?? "");

        InlineDrawing inlineDrawing = new(
            builderServices.IdFactory.NextDrawingId(),
            size,
            textStyle,
            image
        );

        return [inlineDrawing];
    }

    private static Size ToSize(this WDrawing.Extent? extent)
    {
        float width = extent?.Cx.EmuToPoint() ?? 0;
        float height = extent?.Cy.EmuToPoint() ?? 0;
        return new Size(width, height);
    }
}
