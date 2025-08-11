using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class SpecialCharactersRenderer
{
    public static void RenderSpecialCharacter(
        this Layout layout,
        XGraphics graphics,
        RenderOptions options)
    {
        if (options.RenderParagraphCharacter && layout is LineLayout lineLayout && lineLayout.Decoration != LineDecoration.None)
        {
            lineLayout.DecorationText.RenderText(graphics, options);
        }
    }
}
