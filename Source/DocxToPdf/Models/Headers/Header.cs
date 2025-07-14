using System;
using System.Linq;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Paragraphs;

namespace Proxoft.DocxToPdf.Models.Headers;

internal class Header(
    PageContextElement[] childs,
    PageMargin pageMargin) : HeaderBase(pageMargin)
{
    private readonly PageContextElement[] _childs = childs;

    public override void Prepare(IPage page)
    {
        PagePosition pagePosition = new(page.PageNumber);
        Rectangle region = page
            .GetPageRegion()
            .Crop(this.PageMargin.Header, this.PageMargin.Right, this.PageMargin.Footer, this.PageMargin.Left);

        PageContext context = new(pagePosition, region, page.DocumentVariables);

        PageContext childContextRequest(PagePosition pagePosition, PageContextElement child)
            => OutOfPageContextFactory(page);

        double absoluteHeight = page.Configuration.Height;

        double spaceAfterPrevious = 0.0;
        foreach (PageContextElement child in _childs)
        {
            child.Prepare(context, childContextRequest);
            PageRegion lastRegion = child.LastPageRegion;
            spaceAfterPrevious = child.CalculateSpaceAfter(_childs);

            double cropFromTop = Math.Min(lastRegion.Region.Height + spaceAfterPrevious, absoluteHeight - 0.001);
            context = context.CropFromTop(cropFromTop);
        }

        PageRegion[] pageRegionsOfChilds = [.. _childs.SelectMany(c => c.PageRegions.Where(pr => pr.PagePosition.PageNumber == page.PageNumber))];

        PageRegion boundingRegion = pageRegionsOfChilds
            .UnionPageRegions(Margin.None)
            .Single();

        if(boundingRegion.Region.BottomY < this.PageMargin.Top)
        {
            Rectangle resized = new(
                boundingRegion.Region.TopLeft,
                boundingRegion.Region.Width,
                this.PageMargin.MinimalHeaderHeight);

            boundingRegion = new PageRegion(
                boundingRegion.PagePosition, resized);
        }

        this.ResetPageRegions([ boundingRegion ]);
    }

    public override void Render(IRenderer renderer)
    {
        _childs.Render(renderer);
        this.RenderBorders(renderer, renderer.Options.HeaderBorders);
    }

    private static PageContext OutOfPageContextFactory(IPage page)
    {
        Rectangle region = new(0, page.Configuration.Height + 1, 1000000, 1000000);
        return new PageContext(PagePosition.None, region, new DocumentVariables(0));
    }
}
