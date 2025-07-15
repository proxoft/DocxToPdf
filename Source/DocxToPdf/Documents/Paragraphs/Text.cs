namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Text(
    ModelId Id,
    string text
) : Element(Id);
