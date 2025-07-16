using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record EmptyLayout(ModelReference Source, Rectangle BoundingBox) : ElementLayout(Source, BoundingBox);