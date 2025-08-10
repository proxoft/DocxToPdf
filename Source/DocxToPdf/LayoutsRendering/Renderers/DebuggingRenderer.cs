using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class DebuggingRenderer
{
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
