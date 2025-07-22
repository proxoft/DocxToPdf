using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record EmptyLayout(
    Rectangle BoundingBox,
    Borders Borders
) : ElementLayout(BoundingBox, Borders);