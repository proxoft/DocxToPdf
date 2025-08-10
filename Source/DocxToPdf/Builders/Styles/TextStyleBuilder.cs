using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

using Col = Proxoft.DocxToPdf.Documents.Styles.Color;
using Drawing = DocumentFormat.OpenXml.Drawing;

namespace Proxoft.DocxToPdf.Builders.Styles;

internal static class TextStyleBuilder
{
    public static TextStyle CreateDefaultTextStyle(this DocDefaults? docDefaults, Drawing.Theme? theme)
    {
        if (docDefaults?.RunPropertiesDefault is null)
        {
            return new TextStyle("Arial", 11, FontDecoration.None, Col.Black, Col.Empty);
        }

        TextStyle textStyle = docDefaults.RunPropertiesDefault.CreateTextStyle(theme);
        return textStyle;
    }

    public static TextStyle CreateTextStyle(
        this TextStyle defaultTextStyle,
        RunProperties? runProperties,
        StyleRunProperties[] styles)
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

        FontDecoration fontDecoration = defaultTextStyle.FontDecoration.GetFontDecoration(runProperties, styles);

        return new TextStyle(
            fontFamily,
            fontSize,
            fontDecoration,
            defaultTextStyle.Brush,
            defaultTextStyle.Background
        );
    }

    private static TextStyle CreateTextStyle(this RunPropertiesDefault runPropertiesDefault, Drawing.Theme? theme)
    {
        string typeFace = runPropertiesDefault.GetTypeFace(theme);
        FontDecoration fontDecoration = runPropertiesDefault?.RunPropertiesBaseStyle.GetFontDecoretion() ?? FontDecoration.None; ;
        float size = runPropertiesDefault?.RunPropertiesBaseStyle?.FontSize.ToFloat(11) ?? 11;
        Col brush = Col.Black; // runPropertiesDefault?.RunPropertiesBaseStyle?.Color.ToColor() ?? Col.Black;

        return new TextStyle(typeFace, size, fontDecoration, brush, Col.Empty);
    }

    private static string GetTypeFace(this RunPropertiesDefault? runPropertiesDefault, Drawing.Theme? theme)
    {
        string? x = runPropertiesDefault?.RunPropertiesBaseStyle?.RunFonts?.Ascii;
        string? y = theme?.ThemeElements?.FontScheme?.MinorFont?.LatinFont?.Typeface;

        string typeface = x
            ?? y
            ?? "Arial";

        return typeface;
    }

    private static FontDecoration GetFontDecoretion(this RunPropertiesBaseStyle? runPropertiesBase)
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
        RunProperties? runProperties,
        StyleRunProperties[] styles)
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
}
