namespace Proxoft.DocxToPdf.Documents.Styles.Texts;

internal record TextStyle(
    string FontFamily,
    float FontSize,
    FontDecoration FontDecoration,
    Color Brush,
    Color Background
);
