namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Word(
    ModelId Id,
    string text
) : Element(Id);
