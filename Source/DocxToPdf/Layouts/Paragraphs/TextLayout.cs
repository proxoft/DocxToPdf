using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record TextLayout(
    ModelReference Source,
    Rectangle BoundingBox,
    double BaselineOffset,
    Text Text,
    Borders Borders) : ElementLayout(Source, BoundingBox, Borders);
