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
