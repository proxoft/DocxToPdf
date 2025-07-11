using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Core.Pages;
using Proxoft.DocxToPdf.Extensions;

namespace Proxoft.DocxToPdf.Models.Sections;

internal class HeaderFooterConfiguration
{
    public static readonly HeaderFooterConfiguration Empty = new(null, false, [], []);

    private readonly HeaderFooterRef[] _headers;
    private readonly HeaderFooterRef[] _footers;
    private readonly MainDocumentPart? _mainDocument;
    private readonly bool _hasTitlePage;

    private HeaderFooterConfiguration(
        MainDocumentPart? mainDocument,
        bool hasTitlePage,
        HeaderFooterRef[] headers,
        HeaderFooterRef[] footers)
    {
        _mainDocument = mainDocument;
        _hasTitlePage = hasTitlePage;
        _headers = headers;
        _footers = footers;
    }

    private bool UseEvenOddHeadersAndFooters => _mainDocument?.DocumentSettingsPart?.EvenOddHeadersAndFooters() ?? false;

    public Word.Header? FindHeader(PageNumber pageNumber)
    {
        string? referenceId = this.GetHeaderReferenceId(pageNumber);
        var header = _mainDocument?.FindHeader(referenceId);
        return header;
    }

    public Word.Footer? FindFooter(PageNumber pageNumber)
    {
        var referenceId = this.GetFooterReferenceId(pageNumber);
        var footer = _mainDocument?.FindFooter(referenceId);
        return footer;
    }

    public HeaderFooterConfiguration Inherited(
        MainDocumentPart mainDocument,
        bool hasTitlePage,
        HeaderFooterRef[] headers,
        HeaderFooterRef[] footers)
    {
        HeaderFooterRef[] h = headers.Length > 0
            ? headers
            : _headers;

        HeaderFooterRef[] f = footers.Length > 0
            ? footers
            : _footers;

        return new HeaderFooterConfiguration(mainDocument, hasTitlePage, h, f);
    }

    private string? GetHeaderReferenceId(PageNumber pageNumber) =>
        this.GetReferenceId(_headers, pageNumber);

    private string? GetFooterReferenceId(PageNumber pageNumber) =>
        this.GetReferenceId(_footers, pageNumber);

    private string? GetReferenceId(HeaderFooterRef[] references, int pageNumber)
    {
        if (_hasTitlePage && pageNumber == 1)
        {
            return references.FirstOrDefault(r => r.Type == Word.HeaderFooterValues.First)?.Id
                ?? references.FirstOrDefault(r => r.Type == Word.HeaderFooterValues.Default)?.Id;
        }

        if (!this.UseEvenOddHeadersAndFooters || pageNumber % 2 == 1)
        {
            return references.FirstOrDefault(r => r.Type == Word.HeaderFooterValues.Default)?.Id;
        }

        return references.FirstOrDefault(r => r.Type == Word.HeaderFooterValues.Even)?.Id
            ?? references.FirstOrDefault(r => r.Type == Word.HeaderFooterValues.Default)?.Id;
    }
}
