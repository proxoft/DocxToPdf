﻿using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Sections
{
    internal class SectionProperties
    {
        public static readonly SectionProperties Empty = new SectionProperties(
            PageConfiguration.Empty,
            HeaderFooterConfiguration.Empty,
            PageMargin.PageNone,
            false);

        public SectionProperties(
            PageConfiguration pageConfiguration,
            HeaderFooterConfiguration headerFooterConfiguration,
            PageMargin margin,
            bool StartOnNextPage)
        {
            this.PageConfiguration = pageConfiguration;
            this.HeaderFooterConfiguration = headerFooterConfiguration;
            this.Margin = margin;
            this.StartOnNextPage = StartOnNextPage;
        }

        public PageConfiguration PageConfiguration { get; }
        public HeaderFooterConfiguration HeaderFooterConfiguration { get; }
        public PageMargin Margin { get; }
        public bool StartOnNextPage { get; }
        public bool HasTitlePage { get; }
    }
}
