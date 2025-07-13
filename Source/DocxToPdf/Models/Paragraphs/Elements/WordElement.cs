using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements;

internal class WordElement(string content, TextStyle textStyle) : TextElement(content, string.Empty, textStyle)
{
}
