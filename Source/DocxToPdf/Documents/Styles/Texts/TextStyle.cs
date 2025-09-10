using System;

namespace Proxoft.DocxToPdf.Documents.Styles.Texts;

internal record TextStyle(
    string FontFamily,
    float FontSize,
    FontDecoration FontDecoration,
    Color Brush,
    Color Background
)
{
    public static readonly TextStyle Default = new("Arial", 11, FontDecoration.None, Color.Black, Color.Empty);
}

internal static class TextStyleOperations
{
    public static TextStyle ResizeFont(this TextStyle style, float fontSizeDelta) =>
        style with
        {
            FontSize = Math.Max(1, style.FontSize + fontSizeDelta)
        };
}
