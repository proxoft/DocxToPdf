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

        bb.RenderBorder(layout.Partition, layout.Borders, graphics);
    }

    public static void RenderBorder(
        this Rectangle rectangle,
        LayoutPartition layoutPartition,
        Borders borders,
        XGraphics graphics)
    {
        rectangle.LeftLine.RenderBorder(borders.Left, graphics);
        if (layoutPartition.HasFlag(LayoutPartition.Start))
        {
            rectangle.TopLine.RenderBorder(borders.Top, graphics);
        }

        rectangle.RightLine.RenderBorder(borders.Right, graphics);
        if (layoutPartition.HasFlag(LayoutPartition.End))
        {
            rectangle.BottomLine.RenderBorder(borders.Bottom, graphics);
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
