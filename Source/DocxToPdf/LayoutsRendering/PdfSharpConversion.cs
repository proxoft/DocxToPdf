using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.LayoutsRendering;

internal static class PdfSharpConversion
{
    public static XPoint ToXPoint(this Position position) =>
        new(position.X, position.Y);

    public static XFont ToXFont(this TextStyle textStyle)
    {
        XFontStyle fs = textStyle.FontDecoration.ToXFontStyle();
        return new XFont(textStyle.FontFamily, textStyle.FontSize, fs);
    }

    public static XPen ToXPen(this BorderStyle borderStyle)
    {
        (int r, int g, int b) = borderStyle.Color.Rgb;
        float width = borderStyle.LineStyle == LineStyle.None
            ? 0
            : borderStyle.Width;

        XColor color = borderStyle.LineStyle == LineStyle.None
            ? XColor.Empty
            : XColor.FromArgb(0, r, g, b);

        return new(color, width)
        {
            DashStyle = borderStyle.LineStyle.ToXDashStyle()
        };
    }

    public static XRect ToXRect(this Rectangle rectangle) =>
        new (
            new XPoint(rectangle.X, rectangle.Y),
            new XSize(rectangle.Width, rectangle.Height)
        );

    public static XBrush ToXBrush(this Color color)
    {
        if (color == Color.Empty) return XBrushes.Transparent;

        (int r, int g, int b) = color.Rgb;
        XColor col = XColor.FromArgb(r, g, b);
        return new XSolidBrush(col);

    }

    private static XDashStyle ToXDashStyle(this LineStyle lineStyle) =>
        lineStyle switch
        {
            LineStyle.None => XDashStyle.Solid,
            LineStyle.Solid => XDashStyle.Solid,
            LineStyle.Dashed => XDashStyle.Dash,
            LineStyle.Dotted => XDashStyle.Dot,
            LineStyle.DotDash => XDashStyle.DashDot,
            LineStyle.DotDotDash => XDashStyle.DashDotDot,
            _ => XDashStyle.Solid
        };

    private static XFontStyle ToXFontStyle(this FontDecoration fontDecoration)
    {
        XFontStyle style = XFontStyle.Regular;
        if(fontDecoration.HasFlag(FontDecoration.Bold))
            style |= XFontStyle.Bold;

        if (fontDecoration.HasFlag(FontDecoration.Italic))
            style |= XFontStyle.Italic;

        if (fontDecoration.HasFlag(FontDecoration.Underline))
            style |= XFontStyle.Underline;

        if (fontDecoration.HasFlag(FontDecoration.Strikethrough))
            style |= XFontStyle.Strikeout;

        return style;
    }
}
