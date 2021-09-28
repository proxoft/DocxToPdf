using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs
{
    internal class TabElement : TextElement
    {
        public TabElement(TextStyle textStyle) : base("    ", "····", textStyle)
        {
        }
    }
}
