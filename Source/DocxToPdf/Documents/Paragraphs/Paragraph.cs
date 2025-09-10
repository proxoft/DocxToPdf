using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;

namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Paragraph(
    ModelId Id,
    Element[] Elements,
    FixedDrawing[] FixedDrawings,
    ParagraphStyle Style
) : Model(Id);
