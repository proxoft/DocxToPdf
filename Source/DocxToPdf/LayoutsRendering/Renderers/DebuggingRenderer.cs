using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class DebuggingRenderer
{
    public static void RenderDebuggingBorder(
        this Layout layout,
        Position offset,
        XGraphics graphics,
        RenderOptions options)
    {
        BorderStyle borderStyle = options.GetBorderStyle(layout);
        if (borderStyle == BorderStyle.None)
        {
            return;
        }

        Rectangle bb = layout.BoundingBox
            .MoveXBy(offset.X)
            .MoveYBy(offset.Y);

        bb.RenderBorder(layout.Partition, Borders.All(borderStyle), graphics);
    }
}
