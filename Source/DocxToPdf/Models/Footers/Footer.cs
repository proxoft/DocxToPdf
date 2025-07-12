using System;
using System.Linq;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Paragraphs;

namespace Proxoft.DocxToPdf.Models.Footers;

internal class Footer(
    PageContextElement[] childs,
    PageMargin pageMargin) : FooterBase(pageMargin)
{
    private readonly PageContextElement[] _childs = childs;
    private Point _pageOffset = Point.Zero;

    public override void Prepare(IPage page)
    {
        PagePosition pagePosition = new(page.PageNumber);
        Rectangle region = page
            .GetPageRegion()
            .Crop(page.Margin.Top, this.PageMargin.Right, this.PageMargin.Footer, this.PageMargin.Left);

        PageContext context = new(pagePosition, region, page.DocumentVariables);

        PageContext childContextRequest(PagePosition pagePosition, PageContextElement child)
            => this.OutOfPageContextFactory(page);

        var absoluteHeight = page.Configuration.Height;

        double spaceAfterPrevious = 0.0;
        foreach (var child in _childs)
        {
            child.Prepare(context, childContextRequest);
            PageRegion lastRegion = child.LastPageRegion;
            spaceAfterPrevious = child.CalculateSpaceAfter(_childs);

            double cropFromTop = Math.Min(lastRegion.Region.Height + spaceAfterPrevious, absoluteHeight - 0.001);
            context = context.CropFromTop(cropFromTop);
        }

        PageRegion boundingRegion = _childs
            .SelectMany(c => c.PageRegions.Where(pr => pr.PagePosition.PageNumber == page.PageNumber))
            .UnionPageRegions(Margin.None)
            .ToArray()
            .Single();

        double offsetY = page.Configuration.Height
            - page.Margin.Top
            - Math.Max(boundingRegion.Region.Height, this.PageMargin.FooterHeight)
            - this.PageMargin.Footer;

        _pageOffset = new Point(0, offsetY);
        foreach(var child in _childs)
        {
            child.SetPageOffset(_pageOffset);
        }

        if (boundingRegion.Region.Height < this.PageMargin.FooterHeight)
        {
            var resized = new Rectangle(boundingRegion.Region.TopLeft, boundingRegion.Region.Width, this.PageMargin.FooterHeight);
            boundingRegion = new PageRegion(boundingRegion.PagePosition, resized);
        }

        this.ResetPageRegions([ boundingRegion ]);
    }

    public override void Render(IRenderer renderer)
    {
        _childs.Render(renderer);
        this.RenderBorders(renderer, renderer.Options.FooterBorders, pageOffset: _pageOffset);
    }

    private PageContext OutOfPageContextFactory(IPage page)
    {
        Rectangle region = new(0, page.Configuration.Height + 1, 1000000, 1000000);
        return new PageContext(PagePosition.None, region, new DocumentVariables(0));
    }
}
