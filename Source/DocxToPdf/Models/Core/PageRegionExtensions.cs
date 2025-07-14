using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Core;

internal static class PageRegionExtensions
{
    public static IEnumerable<PageRegion> UnionPageRegions(
        this IEnumerable<PageRegion> pageRegions,
        Margin contentMargin)
    {
        PageRegion[] unioned = [.. pageRegions
            .GroupBy(pr => pr.PagePosition)
            .Select(grp =>
            {
                Rectangle rectangle = Rectangle.Union(grp.Select(r => r.Region));
                return new PageRegion(grp.Key, rectangle);
            })
            .Select((pr, i) =>
            {
                double top = i == 0
                    ? contentMargin.Top
                    : 0;
                PageRegion npr = new(pr.PagePosition, pr.Region.Inflate(top, contentMargin.Right, 0, contentMargin.Left));
                return npr;
            })];

        PageRegion last = unioned.Last();
        unioned[^1] = new PageRegion(last.PagePosition, last.Region.Inflate(0, 0, contentMargin.Bottom, 0));

        return unioned;
    }

    public static IEnumerable<PageRegion> UnionThroughColumns(
        this IEnumerable<PageRegion> pageRegions)
    {
        PageRegion[] unioned = [.. pageRegions
            .GroupBy(pr => pr.PagePosition.PageNumber)
            .Select(grp =>
            {
                Rectangle rectangle = Rectangle.Union(grp.Select(r => r.Region));
                return new PageRegion(new PagePosition(grp.Key, PageColumn.First, new PageColumn(1)), rectangle);
            })];

        return unioned;
    }
}
