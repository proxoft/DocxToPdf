using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class BorderRenderer
{
    public static void RenderBorder(this Layout layout, XGraphics graphics)
    {
        if (layout.Borders == Borders.None)
        {
            return;
        }

        layout.BoundingBox.LeftLine.RenderBorder(layout.Borders.Left, graphics);
        if (layout.Partition is LayoutPartition.Start or LayoutPartition.StartEnd)
        {
            layout.BoundingBox.TopLine.RenderBorder(layout.Borders.Top, graphics);
        }

        layout.BoundingBox.RightLine.RenderBorder(layout.Borders.Right, graphics);
        if (layout.Partition is LayoutPartition.End or LayoutPartition.StartEnd)
        {
            layout.BoundingBox.BottomLine.RenderBorder(layout.Borders.Bottom, graphics);
        }
    }

    private static void RenderBorder(this (Position start, Position end) line, BorderStyle borderStyle, XGraphics graphics)
    {
        if (borderStyle == BorderStyle.None
            || borderStyle.Width == 0
            || borderStyle.LineStyle == LineStyle.None)
        {
            return;
        }

        XPen pen = borderStyle.ToXPen();
        graphics.DrawLine(pen, line.start.ToXPoint(), line.end.ToXPoint());
    }
}
