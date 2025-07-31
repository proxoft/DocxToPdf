using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;

namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Paragraph(
    ModelId Id,
    Element[] Elements,
    ParagraphStyle Style
) : Model(Id);
