using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class DebuggingRenderer
{
    public static void RenderSpecialCharacter(
        this Layout layout,
        XGraphics graphics,
        RenderOptions options)
    {
        if (options.RenderParagraphCharacter && layout is LineLayout lineLayout && lineLayout.IsLastLineOfParagraph)
        {
            lineLayout.SpecialCharacter.RenderText(graphics, options);
        }
    }

    public static void RenderDebuggingBorder(
        this Layout layout,
        XGraphics graphics,
        RenderOptions options)
    {
        BorderStyle borderStyle = options.GetBorderStyle(layout);
        if (borderStyle == BorderStyle.None)
        {
            return;
        }

        XPen pen = borderStyle.ToXPen();
        XRect rect = layout.BoundingBox.ToXRect();

        graphics.DrawRectangle(pen, rect);
    }
}
