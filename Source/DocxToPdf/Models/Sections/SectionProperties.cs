using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Sections;

internal class SectionProperties(
    PageConfiguration pageConfiguration,
    HeaderFooterConfiguration headerFooterConfiguration,
    PageMargin margin,
    bool StartOnNextPage)
{
    public static readonly SectionProperties Empty = new(
        PageConfiguration.Empty,
        HeaderFooterConfiguration.Empty,
        PageMargin.PageNone,
        false);

    public PageConfiguration PageConfiguration { get; } = pageConfiguration;
    public HeaderFooterConfiguration HeaderFooterConfiguration { get; } = headerFooterConfiguration;
    public PageMargin Margin { get; } = margin;
    public bool StartOnNextPage { get; } = StartOnNextPage;
    public bool HasTitlePage { get; }
}
