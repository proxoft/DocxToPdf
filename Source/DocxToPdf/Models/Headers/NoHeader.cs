using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Headers;

internal class NoHeader(PageMargin pageMargin) : HeaderBase(pageMargin)
{
    public override void Prepare(IPage page)
    {
        PagePosition pagePosition = new(page.PageNumber);
        Rectangle headerRegion = new(
            this.PageMargin.Left,
            this.PageMargin.Header,
            page.Configuration.Width - this.PageMargin.HorizontalMargins,
            this.PageMargin.MinimalHeaderHeight);

        this.SetPageRegion(new PageRegion(pagePosition, headerRegion));
    }

    public override void Render(IRenderer renderer)
    {
        this.RenderBorders(renderer, renderer.Options.HeaderBorders);
    }
}
