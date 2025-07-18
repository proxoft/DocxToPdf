using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record TextLayout(
    ModelReference Source,
    Rectangle BoundingBox,
    double BaselineOffset,
    Text Text) : ElementLayout(Source, BoundingBox)
{
}
