using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs
{
    internal class WordElement : TextElement
    {
        public WordElement(string content, TextStyle textStyle) : base(content, string.Empty, textStyle)
        {
        }
    }
}
