using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record FixedImageLayout(
    ModelId ModelId,
    byte[] Content,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(ModelId, BoundingBox, Borders, LayoutPartition.StartEnd);