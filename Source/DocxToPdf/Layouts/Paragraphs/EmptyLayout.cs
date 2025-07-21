using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record EmptyLayout(
    ModelReference Source,
    Rectangle BoundingBox,
    Borders Borders
) : ElementLayout(Source, BoundingBox, Borders);