using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record ParagraphLayout(
    ModelReference Source,
    LineLayout[] Lines,
    Rectangle BoundingBox
) : Layout(Source, BoundingBox);
