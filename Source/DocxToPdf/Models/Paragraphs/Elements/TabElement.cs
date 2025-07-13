using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements;

internal class TabElement(TextStyle textStyle) : TextElement("    ", "····", textStyle)
{
}
