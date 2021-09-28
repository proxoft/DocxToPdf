using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Styles;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Models.Headers.Builders
{
    internal static class HeaderFactory
    {
        public static HeaderBase CreateInheritedHeader(PageMargin pageMargin)
        {
            return new NoHeader(pageMargin);
        }

        public static HeaderBase CreateHeader(
            this Word.Header wordHeader,
            PageMargin pageMargin,
            IImageAccessor imageAccessor,
            IStyleFactory styleFactory)
        {
            if(wordHeader == null)
            {
                return new NoHeader(pageMargin);
            }

            var childElements = wordHeader.RenderableChildren().CreatePageElements(imageAccessor, styleFactory);
            return new Header(childElements, pageMargin);
        }
    }
}
