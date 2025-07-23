using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record TextLayout(
    Rectangle BoundingBox,
    float BaselineOffset,
    Text Text,
    Borders Borders) : ElementLayout(BoundingBox, Borders);
