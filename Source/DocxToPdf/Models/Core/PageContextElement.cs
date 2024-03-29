﻿using System;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models
{
    internal abstract class PageContextElement : PageElement
    {
        public abstract void SetPageOffset(Point pageOffset);

        public abstract void Prepare(
            PageContext pageContext,
            Func<PagePosition, PageContextElement, PageContext> nextPageContextFactory);
    }
}
