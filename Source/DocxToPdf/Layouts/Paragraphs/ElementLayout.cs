using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal abstract record ElementLayout(ModelReference Source, Rectangle BoundingBox) : Layout(Source, BoundingBox);