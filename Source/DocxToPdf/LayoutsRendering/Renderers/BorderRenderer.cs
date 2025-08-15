using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class BorderRenderer
{
    public static void RenderBorder(this Layout layout, Position offset, XGraphics graphics)
    {
        if (layout.Borders == Borders.None)
        {
            return;
        }

        Rectangle bb = layout.BoundingBox
            .MoveX(offset.X)
            .MoveY(offset.Y);

        bb.LeftLine.RenderBorder(layout.Borders.Left, graphics);
        if (layout.Partition is LayoutPartition.Start or LayoutPartition.StartEnd)
        {
            bb.TopLine.RenderBorder(layout.Borders.Top, graphics);
        }

        bb.RightLine.RenderBorder(layout.Borders.Right, graphics);
        if (layout.Partition is LayoutPartition.End or LayoutPartition.StartEnd)
        {
            bb.BottomLine.RenderBorder(layout.Borders.Bottom, graphics);
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
