using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal abstract record ElementLayout(
    Rectangle BoundingBox,
    Borders Borders
) : Layout(BoundingBox, Borders);