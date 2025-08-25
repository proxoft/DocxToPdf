using System.IO;
using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class ImageRenderer
{
    public static void RenderImage(this ImageLayout layout, Position offset, XGraphics graphics)
    {
        if(layout.Data.Length == 0)
        {
            RenderNoImagePlaceholder(layout.BoundingBox.MoveBy(offset.X, offset.Y), graphics);
            return;
        }

        using MemoryStream ms = new(layout.Data);
        XImage image = XImage.FromStream(ms);
        Position offsetPosition = layout.BoundingBox.TopLeft
            .ShiftX(offset.X)
            .ShiftY(offset.Y);

        graphics.DrawImage(image, offsetPosition.X, offsetPosition.Y, layout.Size.Width, layout.Size.Height);
    }

    private static void RenderNoImagePlaceholder(Rectangle rectangle, XGraphics graphics)
    {
        XPen pen = new(XColors.Black, 1);
        graphics.DrawRectangle(pen, rectangle.ToXRect());
        graphics.DrawLine(pen, rectangle.TopLeft.ToXPoint(), rectangle.BottomRight.ToXPoint());
        graphics.DrawLine(pen, rectangle.BottomLeft.ToXPoint(), rectangle.TopRight.ToXPoint());
    }
}
