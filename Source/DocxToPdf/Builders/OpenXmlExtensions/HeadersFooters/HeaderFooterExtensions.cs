using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.HeadersFooters;

internal static class HeaderFooterExtensions
{
    public static HeaderFooterType ToHeaderFooterType(this OpenXml.EnumValue<Word.HeaderFooterValues> headerFooterValues)
    {
        if (headerFooterValues is null) return HeaderFooterType.Default;
        if (headerFooterValues == Word.HeaderFooterValues.First) return HeaderFooterType.First;
        if (headerFooterValues == Word.HeaderFooterValues.Even) return HeaderFooterType.Even;
        return HeaderFooterType.Default;
    }
}
