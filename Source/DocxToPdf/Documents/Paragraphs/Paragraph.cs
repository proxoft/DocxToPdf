namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Paragraph(
    ModelId Id,
    Element[] Elements
) : Model(Id);
