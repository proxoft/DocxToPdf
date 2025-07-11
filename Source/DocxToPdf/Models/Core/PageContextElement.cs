using System;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;

namespace Proxoft.DocxToPdf.Models;

internal abstract class PageContextElement : PageElement
{
    public abstract void SetPageOffset(Point pageOffset);

    public abstract void Prepare(
        PageContext pageContext,
        Func<PagePosition, PageContextElement, PageContext> nextPageContextFactory);
}
