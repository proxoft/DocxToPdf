using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Footers;
using Proxoft.DocxToPdf.Documents.Headers;

namespace Proxoft.DocxToPdf.Documents.Sections;

internal enum PageNumberType
{
    Default, // == Odd
    First,
    Even,
}

internal record HeaderFooterConfiguration(
    bool HasTitlePage,
    bool UseEvenOddHeader,
    Dictionary<PageNumberType, Header> Headers,
    Dictionary<PageNumberType, Footer> Footers
)
{
    public static readonly HeaderFooterConfiguration None = new(false, false, [], []);
}
