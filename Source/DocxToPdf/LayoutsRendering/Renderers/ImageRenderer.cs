using System.IO;
using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class ImageRenderer
{
    public static void RenderImage(this FixedImageLayout layout, Position offset, XGraphics graphics)
    {
        if (layout.Content.Length == 0)
        {
            RenderNoImagePlaceholder(layout.BoundingBox.MoveBy(offset.X, offset.Y), graphics);
            return;
        }

        Position position = layout.BoundingBox.TopLeft
            .Shift(offset.X, offset.Y)
            .Shift(layout.Padding.Left, layout.Padding.Top);

        Size size = layout.BoundingBox.Size
            .Clip(layout.Padding);

        layout.Content.RenderImage(position, size, graphics);
    }

    public static void RenderImage(this ImageLayout layout, Position offset, XGraphics graphics)
    {
        if(layout.Data.Length == 0)
        {
            RenderNoImagePlaceholder(layout.BoundingBox.MoveBy(offset.X, offset.Y), graphics);
            return;
        }

        Position position = layout.BoundingBox.TopLeft
            .ShiftX(offset.X)
            .ShiftY(offset.Y);

        layout.Data.RenderImage(position, layout.Size, graphics);
    }

    private static void RenderImage(this byte[] content, Position position, Size size, XGraphics graphics)
    {
        using MemoryStream ms = new(content);
        XImage image = XImage.FromStream(ms);
        graphics.DrawImage(image, position.X, position.Y, size.Width, size.Height);
    }

    private static void RenderNoImagePlaceholder(Rectangle rectangle, XGraphics graphics)
    {
        XPen pen = new(XColors.Black, 1);
        graphics.DrawRectangle(pen, rectangle.ToXRect());
        graphics.DrawLine(pen, rectangle.TopLeft.ToXPoint(), rectangle.BottomRight.ToXPoint());
        graphics.DrawLine(pen, rectangle.BottomLeft.ToXPoint(), rectangle.TopRight.ToXPoint());
    }
}
