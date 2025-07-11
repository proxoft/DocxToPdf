using System.Drawing;
using System.Drawing.Drawing2D;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf;

internal static class BorderTypeConversions
{
    public static Pen? ToPen(this Word.BorderType? border, Pen? defaultIfNull = null)
    {
        if (border is null)
        {
            return defaultIfNull;
        }

        var color = border.Color.ToColor();
        var width = border.Size.EpToPoint();
        var val = border.Val?.Value ?? Word.BorderValues.Single;
        var pen = new Pen(color, width);
        pen.UpdateStyle(val);
        return pen;
    }

    private static void UpdateStyle(this Pen pen, Word.BorderValues borderValue)
    {
        if(borderValue == Word.BorderValues.Nil || borderValue == Word.BorderValues.None)
        {
            pen.Color = Color.Transparent;
            pen.Width = 0;
            return;
        }

        pen.DashStyle = borderValue.ToDashStyle();
    }

    private static DashStyle ToDashStyle(this Word.BorderValues borderValue)
    {
        if (borderValue == Word.BorderValues.Nil
            || borderValue == Word.BorderValues.None
            || borderValue == Word.BorderValues.Single
            || borderValue == Word.BorderValues.Thick)
        {
            return DashStyle.Solid;
        }

        if(borderValue == Word.BorderValues.Dotted)
        {
            return DashStyle.Dot;
        }

        if (borderValue == Word.BorderValues.DashSmallGap || borderValue == Word.BorderValues.Dashed)
        {
            return DashStyle.Dash;
        }

        if (borderValue == Word.BorderValues.DotDash)
        {
            return DashStyle.DashDot;
        }

        if (borderValue == Word.BorderValues.DotDotDash)
        {
            return DashStyle.DashDotDot;
        }

        return DashStyle.Solid;
    }
}
