using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Structs;
using Drawing = System.Drawing;

namespace Proxoft.DocxToPdf.Rendering.Helpers;

internal static class TextConversions
{
    public static XFont ToXFont(this TextStyle textStyle)
    {
        var f = textStyle.Font;
        return new XFont(f.FontFamily.Name, f.Size, (XFontStyle)f.Style);
    }

    public static XBrush ToXBrush(this TextStyle textStyle)
    {
        var color = XColor.FromArgb(textStyle.Brush.ToArgb());
        return new XSolidBrush(color);
    }

    public static XPen GetXPen(this Line line) =>
        line.Pen.ToXPen();

    public static XPen ToXPen(this Drawing.Pen? pen) =>
        pen is null 
            ? new XPen(XColor.Empty)
            : new (pen.Color.ToXColor(), pen.Width)
                {
                    DashStyle = (XDashStyle)pen.DashStyle
                };
}
