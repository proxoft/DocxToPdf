using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;

internal record InlineDrawing(
    ModelId Id,
    Size Size,
    TextStyle TextStyle,
    byte[] Image) : Element(Id, TextStyle)
{
}
