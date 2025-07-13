using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements;

internal class SpaceElement(TextStyle textStyle) : TextElement(" ", "·", textStyle)
{
}
