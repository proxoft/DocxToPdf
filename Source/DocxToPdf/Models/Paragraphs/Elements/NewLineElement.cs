using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs
{
    internal class NewLineElement : TextElement
    {
        public NewLineElement(TextStyle textStyle) : base(string.Empty, "↵", textStyle)
        {
        }
    }
}
