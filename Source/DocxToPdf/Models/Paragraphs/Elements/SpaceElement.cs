using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs
{
    internal class SpaceElement : TextElement
    {
        public void Stretch()
        {
        }

        public SpaceElement(TextStyle textStyle) : base(" ", "·", textStyle)
        {
        }
    }
}
