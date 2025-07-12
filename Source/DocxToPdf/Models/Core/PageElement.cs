using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Drawing = System.Drawing;

namespace Proxoft.DocxToPdf.Models.Core;

internal abstract class PageElement : IRenderable
{
    public IReadOnlyCollection<PageRegion> PageRegions { get; private set; } = [];

    public PageRegion LastPageRegion => this.PageRegions.LastOrDefault() ?? PageRegion.None;

    public abstract void Render(IRenderer renderer);

    protected void SetPageRegion(PageRegion pageRegion)
    {
        this.PageRegions = [
            .. this.PageRegions
                .Where(pr => pr.PagePosition != pageRegion.PagePosition)
                .Union([ pageRegion ])
                .OrderBy(pr => pr.PagePosition)
        ];
    }

    protected void ResetPageRegions(IEnumerable<PageRegion> pageRegions)
    {
        this.PageRegions = [.. pageRegions];
    }

    protected void ResetPageRegionsFrom(IEnumerable<PageContextElement> children, Margin? contentMargin = null)
    {
        this.PageRegions = [.. children.UnionPageRegions(contentMargin)];
    }

    protected void RenderBorders(IRenderer renderer, Drawing.Pen? pen, Point? pageOffset = null)
    {
        if (pen == null)
        {
            return;
        }

        var index = -1;
        foreach (var pageRegion in this.PageRegions)
        {
            index++;
            var page = renderer.GetPage(pageRegion.PagePosition.PageNumber, pageOffset ?? Point.Zero);
            this.RenderBorder(page, pageRegion.Region, index == 0, index == this.PageRegions.Count - 1, pen);
        }
    }

    private void RenderBorder(IRendererPage page, Rectangle region, bool isFirst, bool isLast, Drawing.Pen pen)
    {
        if (isFirst)
        {
            page.RenderLine(region.TopLine(pen));
        }

        page.RenderLine(region.RightLine(pen));

        if (isLast)
        {
            page.RenderLine(region.BottomLine(pen));
        }

        page.RenderLine(region.LeftLine(pen));
    }
}
