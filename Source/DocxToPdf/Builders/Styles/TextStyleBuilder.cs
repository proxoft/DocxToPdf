using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

using Drawing = DocumentFormat.OpenXml.Drawing;

namespace Proxoft.DocxToPdf.Builders.Styles;

internal static class TextStyleBuilder
{
    public static TextStyle CreateDefaultTextStyle(this Word.DocDefaults? docDefaults, Drawing.Theme? theme)
    {
        if (docDefaults?.RunPropertiesDefault is null)
        {
            return new TextStyle("Arial", 11, FontDecoration.None, Color.Black, Color.Empty);
        }

        TextStyle textStyle = docDefaults.RunPropertiesDefault.CreateTextStyle(theme);
        return textStyle;
    }

    public static TextStyle CreateTextStyle(
        this TextStyle defaultTextStyle,
        Word.RunProperties? runProperties,
        Word.StyleRunProperties[] styles)
    {
        string fontFamily = styles
            .Select(s => s.RunFonts?.Ascii?.Value)
            .Prepend(runProperties?.RunFonts?.Ascii?.Value)
            .Append(defaultTextStyle.FontFamily)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .First()!;

        float fontSize = styles
            .Select(s => s.FontSize.ToFloat(0))
            .Prepend(runProperties?.FontSize.ToFloat(0) ?? 0)
            .Append(defaultTextStyle.FontSize)
            .Where(s => s > 0)
            .First();

        Color fontColor = runProperties.GetFontColor(styles, defaultTextStyle.Brush);
        FontDecoration fontDecoration = defaultTextStyle.FontDecoration.GetFontDecoration(runProperties, styles);

        Color background = runProperties?.Highlight.ToColor() ?? defaultTextStyle.Background;

        return new TextStyle(
            fontFamily,
            fontSize,
            fontDecoration,
            fontColor,
            background
        );
    }

    private static TextStyle CreateTextStyle(this Word.RunPropertiesDefault runPropertiesDefault, Drawing.Theme? theme)
    {
        string typeFace = runPropertiesDefault.GetTypeFace(theme);
        FontDecoration fontDecoration = runPropertiesDefault?.RunPropertiesBaseStyle.GetFontDecoretion() ?? FontDecoration.None; ;
        float size = runPropertiesDefault?.RunPropertiesBaseStyle?.FontSize.ToFloat(11) ?? 11;
        Color brush = runPropertiesDefault?.RunPropertiesBaseStyle?.Color.ToColor() ?? Color.Black;

        return new TextStyle(typeFace, size, fontDecoration, brush, Color.Empty);
    }

    private static string GetTypeFace(this Word.RunPropertiesDefault? runPropertiesDefault, Drawing.Theme? theme)
    {
        string? x = runPropertiesDefault?.RunPropertiesBaseStyle?.RunFonts?.Ascii;
        string? y = theme?.ThemeElements?.FontScheme?.MinorFont?.LatinFont?.Typeface;

        string typeface = x
            ?? y
            ?? "Arial";

        return typeface;
    }

    private static FontDecoration GetFontDecoretion(this Word.RunPropertiesBaseStyle? runPropertiesBase)
    {
        if (runPropertiesBase is null)
        {
            return FontDecoration.None;
        }

        FontDecoration decoration = runPropertiesBase.Bold.BoldDecoration()
           | runPropertiesBase.Italic.ItalicDecoration()
           | runPropertiesBase.Strike.StrikeDecoration()
           | runPropertiesBase.Underline.UnderlineDecoration();
        ;

        return decoration;
    }

    private static FontDecoration GetFontDecoration(
        this FontDecoration fontDecoration,
        Word.RunProperties? runProperties,
        Word.StyleRunProperties[] styles)
    {
        FontDecoration bold = styles
           .Select(s => s.Bold)
           .Prepend(runProperties?.Bold)
           .Where(b => b is not null)
           .Select(b => b.BoldDecoration())
           .FirstOrDefault(fontDecoration & FontDecoration.Bold);

        FontDecoration italic = styles
           .Select(s => s.Italic)
           .Prepend(runProperties?.Italic)
           .Where(i => i is not null)
           .Select(i => i.ItalicDecoration())
           .FirstOrDefault(fontDecoration & FontDecoration.Italic);

        FontDecoration underline = styles
           .Select(s => s.Underline)
           .Prepend(runProperties?.Underline)
           .Where(u => u is not null)
           .Select(u => u.UnderlineDecoration())
           .FirstOrDefault(fontDecoration & FontDecoration.Underline);

        FontDecoration strike = styles
           .Select(s => s.Strike)
           .Prepend(runProperties?.Strike)
           .Where(s => s is not null)
           .Select(s => s.StrikeDecoration())
           .FirstOrDefault(fontDecoration & FontDecoration.Strikethrough);

        return FontDecoration.None
            | bold
            | italic
            | underline
            | strike;
    }

    private static Color GetFontColor(this Word.RunProperties? runProperties, Word.StyleRunProperties[] styles, Color defaultColor)
    {
        Word.Color? c = styles.Select(s => s.Color)
            .Prepend(runProperties?.Color)
            .Where(c => c is not null)
            .FirstOrDefault();

        return c?.ToColor() ?? defaultColor;
    }
}
