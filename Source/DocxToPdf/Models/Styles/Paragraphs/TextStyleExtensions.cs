using System.Collections.Generic;
using System.Drawing;
using Proxoft.DocxToPdf.Core;

using Draw = DocumentFormat.OpenXml.Drawing;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Models.Styles.Paragraphs;

internal static class TextStyleExtensions
{
    public static TextStyle Override(this TextStyle baseStyle, Word.RunProperties? runProperties, IReadOnlyCollection<Word.StyleRunProperties> styleRuns)
    {
        if (runProperties == null && styleRuns.Count == 0)
        {
            return baseStyle;
        }

        Font? font = baseStyle.Font.Override(runProperties, styleRuns);
        var brush = runProperties?.EffectiveColor(styleRuns, baseStyle.Brush);
        var background = runProperties?.Highlight.ToColor();

        return baseStyle.WithChanged(font: font, brush: brush, background: background);
    }

    public static TextStyle CreateTextStyle(this Word.RunPropertiesDefault? runPropertiesDefault, Draw.Theme? theme)
    {
        string typeFace = runPropertiesDefault.GetTypeFace(theme);
        FontStyle fontStyle = runPropertiesDefault?.RunPropertiesBaseStyle.EffectiveFontStyle() ?? FontStyle.Regular;
        double size = runPropertiesDefault?.RunPropertiesBaseStyle?.FontSize?.ToDouble(11) ?? 11;
        Color brush = runPropertiesDefault?.RunPropertiesBaseStyle?.Color.ToColor() ?? Color.Black;

        Font font = new(typeFace, (float)size, fontStyle);
        return new TextStyle(font, brush, Color.Empty);
    }

    private static string GetTypeFace(this Word.RunPropertiesDefault? runPropertiesDefault, Draw.Theme? theme)
    {
        if(runPropertiesDefault is null || theme is null)
        {
            return "Arial";
        }

        string? x = runPropertiesDefault?.RunPropertiesBaseStyle?.RunFonts?.Ascii;
        string? y = theme?.ThemeElements?.FontScheme?.MinorFont?.LatinFont?.Typeface;

        string typeface = x
            ?? y
            ?? "Arial";

        return typeface;
    }

    private static Font Override(this Font font, Word.RunProperties? runProperties, IReadOnlyCollection<Word.StyleRunProperties> styleRuns)
    {
        string typeFace = runProperties.EffectiveTypeFace(styleRuns, font.FontFamily.Name);
        float size = runProperties.EffectiveFontSize(styleRuns, font.Size);
        FontStyle fontStyle = runProperties.EffectiveFontStyle(styleRuns, font.Style);

        return new Font(typeFace, size, fontStyle);
    }
}
