using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models
{
    internal static class PageContextElementExtensions
    {
        public static IEnumerable<PageRegion> UnionPageRegions(this IEnumerable<PageContextElement> elements, Margin contentMargin = null)
            => elements.UnionPageRegionsCore(contentMargin ?? Margin.None);

        private static IEnumerable<PageRegion> UnionPageRegionsCore(this IEnumerable<PageContextElement> elements, Margin contentMargin)
        {
            var pageRegions = elements
                .SelectMany(c => c.PageRegions)
                .UnionPageRegions(contentMargin)
                .ToArray();

            return pageRegions;
        }
    }
}
