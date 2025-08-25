using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;

internal record FixedDrawing(
    ModelId Id,
    byte[] Image,
    Position Offset,
    Size Size,
    Margin Margin
) : Model(Id)
{
}
