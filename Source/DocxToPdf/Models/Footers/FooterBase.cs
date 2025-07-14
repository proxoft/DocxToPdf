using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;

namespace Proxoft.DocxToPdf.Models.Footers;

internal abstract class FooterBase(PageMargin pageMargin) : PageElement
{
    public double Height => this.LastPageRegion.Region.Height;

    public double FooterMargin => this.PageMargin.Footer;

    public double HeightWithFooterMargin => this.Height + this.PageMargin.Footer;

    protected PageMargin PageMargin { get; } = pageMargin;

    public abstract void Prepare(IPage page);
}
