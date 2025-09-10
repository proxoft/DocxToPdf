using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.HeadersFooters;

internal static class HeaderFooterExtensions
{
    public static PageNumberType ToHeaderFooterType(this OpenXml.EnumValue<Word.HeaderFooterValues> headerFooterValues)
    {
        if (headerFooterValues is null) return PageNumberType.Default;
        if (headerFooterValues == Word.HeaderFooterValues.First) return PageNumberType.First;
        if (headerFooterValues == Word.HeaderFooterValues.Even) return PageNumberType.Even;
        return PageNumberType.Default;
    }
}
