using Proxoft.DocxToPdf.Documents.Styles;
using static DocumentFormat.OpenXml.Wordprocessing.HighlightColorValues;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;

internal static class ColorConverter
{
    public static Color ToColor(this Word.Highlight? highlight)
    {
        OpenXml.EnumValue<Word.HighlightColorValues> colorName = highlight?.Val ?? None;
        Color c = colorName.Value.ToColor();
        return c;
    }

    public static Color ToColor(this Word.Color? color)
    {
        if (color is null)
        {
            return Color.Black;
        }

        return color.Val.ToColor();
    }

    public static Color ToColor(this OpenXml.StringValue? color)
    {
        string? hex = color?.Value;
        return new Color(hex ?? "");
    }

    private static Color ToColor(this Word.HighlightColorValues name)
    {
        if (name == None) return Color.Empty;
        if (name == Black) return Color.FromArgb(0, 0, 0);
        if (name == Blue) return Color.FromArgb(0, 0, 0xFF);
        if (name == Cyan) return Color.FromArgb(0, 0xFF, 0xFF);
        if (name == Green) return Color.FromArgb(0, 0xFF, 0);
        if (name == Magenta) return Color.FromArgb(0xFF, 0, 0xFF);
        if (name == Red) return Color.FromArgb(0xFF, 0, 0);
        if (name == Yellow) return Color.FromArgb(0xFF, 0xFF, 0);
        if (name == White) return Color.FromArgb(0xFF, 0xFF, 0xFF);
        if (name == DarkBlue) return Color.FromArgb(0, 0, 0x80);
        if (name == DarkCyan) return Color.FromArgb(0, 0x80, 0x80);
        if (name == DarkGreen) return Color.FromArgb(0, 0x80, 0);
        if (name == DarkMagenta) return Color.FromArgb(0x80, 0, 0x80);
        if (name == DarkRed) return Color.FromArgb(0x80, 0, 0);
        if (name == DarkYellow) return Color.FromArgb(0x80, 0x80, 0);
        if (name == DarkGray) return Color.FromArgb(0x80, 0x80, 0x80);
        if (name == LightGray) return Color.FromArgb(0xC0, 0xC0, 0xC0);
        if (name == White) return Color.FromArgb(0xFF, 0xFF, 0xFF);

        return Color.Empty; // Fallback for unrecognized color
    }
}
