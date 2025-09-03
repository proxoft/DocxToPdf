using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Headers;

namespace Proxoft.DocxToPdf.Documents.Sections;

internal enum HeaderFooterType
{
    Default, // =~ Odd
    First,
    Even,
}

internal record HeaderFooterConfiguration(
    bool HasTitlePage,
    Dictionary<HeaderFooterType, Header> Headers
)
{
    public static readonly HeaderFooterConfiguration None = new(false, []);
}
