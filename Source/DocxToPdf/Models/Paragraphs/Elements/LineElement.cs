using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Paragraphs
{
    internal abstract class LineElement : ParagraphElementBase
    {
        public abstract void Justify(DocumentPosition position, double baseLineOffset, Size lineSpace);

        public abstract double GetBaseLineOffset();
    }
}
