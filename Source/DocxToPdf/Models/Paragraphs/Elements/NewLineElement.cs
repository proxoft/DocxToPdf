using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements;

internal class NewLineElement(TextStyle textStyle) : TextElement(string.Empty, "↵", textStyle)
{
}
