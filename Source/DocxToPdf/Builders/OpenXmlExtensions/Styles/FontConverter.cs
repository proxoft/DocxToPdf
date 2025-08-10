using DocumentFormat.OpenXml.Wordprocessing;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;

internal static class FontConverter
{
    public static float ToFloat(this FontSize? fontSize, float ifNull) =>
        fontSize?.Val is null
        ? ifNull
        : fontSize.Val.HPToPoint(ifNull);

    public static FontDecoration BoldDecoration(this Bold? bold) =>
        bold.OnOffTypeToStyle(FontDecoration.Bold);

    public static FontDecoration ItalicDecoration(this Italic? italic) =>
        italic.OnOffTypeToStyle(FontDecoration.Italic);

    public static FontDecoration StrikeDecoration(this Strike? strike) =>
        strike.OnOffTypeToStyle(FontDecoration.Strikethrough);

    public static FontDecoration UnderlineDecoration(this Underline? underline)
    {
        if (underline?.Val is null)
        {
            return FontDecoration.None;
        }

        return underline.Val.Value != UnderlineValues.None
            ? FontDecoration.Underline
            : FontDecoration.None;
    }

    private static FontDecoration OnOffTypeToStyle(this OnOffType? onOff, FontDecoration onValue)
    {
        if (onOff == null)
        {
            return FontDecoration.None;
        }

        return (onOff.Val?.Value ?? true)
            ? onValue
            : FontDecoration.None;
    }
}
