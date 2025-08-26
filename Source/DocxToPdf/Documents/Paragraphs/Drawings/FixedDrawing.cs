using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;

internal record FixedDrawing(
    ModelId Id,
    byte[] Image,
    Position Offset,
    Size Size,
    Padding Padding
) : Model(Id)
{
}
