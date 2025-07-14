using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;

namespace Proxoft.DocxToPdf.Models.Footers;

internal class NoFooter(PageMargin pageMargin) : FooterBase(pageMargin)
{
    public override void Prepare(IPage page)
    {
        PagePosition pagePosition = new(page.PageNumber);
        Rectangle footerRegion = new(
            this.PageMargin.Left,
            page.Configuration.Height - this.PageMargin.Bottom,
            page.Configuration.Width - this.PageMargin.HorizontalMargins,
            this.PageMargin.FooterHeight);

        this.SetPageRegion(new PageRegion(pagePosition, footerRegion));
    }

    public override void Render(IRenderer renderer)
    {
        this.RenderBorders(renderer, renderer.Options.FooterBorders);
    }
}
