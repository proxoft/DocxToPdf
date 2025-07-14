using System;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;

namespace Proxoft.DocxToPdf.Models.Headers;

internal abstract class HeaderBase(PageMargin pageMargin) : PageElement
{
    protected PageMargin PageMargin { get; } = pageMargin;

    public double TopY => this.LastPageRegion.Region.Y;

    public double BottomY => Math.Max(this.LastPageRegion.Region.BottomY, this.PageMargin.Top);

    public abstract void Prepare(IPage page);
}
