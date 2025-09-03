using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Extensions;

namespace Proxoft.DocxToPdf.Builders.Services;

internal class HeaderFooterAccessor
{
    private readonly MainDocumentPart _mainDocumentPart;

    private HeaderFooterAccessor(MainDocumentPart mainDocumentPart)
    {
        _mainDocumentPart = mainDocumentPart;
    }

    public bool UseEvenOddHeadersAndFooters() =>
        _mainDocumentPart.DocumentSettingsPart?.EvenOddHeadersAndFooters() ?? false;

    public Word.Header? FindHeader(string referenceId) =>
         _mainDocumentPart.FindHeader(referenceId);

    public static HeaderFooterAccessor Create(MainDocumentPart mainDocumentPart) =>
        new(mainDocumentPart);
}
