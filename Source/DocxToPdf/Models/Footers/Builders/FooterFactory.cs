using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Styles;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Models.Footers.Builders
{
    internal static class FooterFactory
    {
        public static FooterBase CreateInheritedFooter(PageMargin previousSectionMargin)
        {
            return new NoFooter(previousSectionMargin);
        }

        public static FooterBase CreateFooter(
            this Word.Footer wordFooter,
            PageMargin pageMargin,
            IImageAccessor imageAccessor,
            IStyleFactory styleFactory)
        {
            if(wordFooter == null)
            {
                return new NoFooter(pageMargin);
            }

            var childElements = wordFooter.RenderableChildren().CreatePageElements(imageAccessor, styleFactory);
            return new Footer(childElements, pageMargin);
        }
    }
}
