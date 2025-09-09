using System;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.LayoutsRendering;

namespace Proxoft.DocxToPdf.Tests.Tools;

internal class DocxToPdfExecutor(
    string docxSourcePattern,
    string pdfOutputPattern,
    RenderOptions options)
{
    private readonly string _docxSourcePattern = docxSourcePattern;
    private readonly string _pdfOutputPattern = pdfOutputPattern;
    private readonly RenderOptions _options = options;

    public PageLayout[] Convert(string fileName)
    {
        PageLayout[] pages = _docxSourcePattern.Replace("{0}", fileName).ReadAndLayoutDocument();
        _pdfOutputPattern.Replace("{0}", fileName).RenderAndSave(pages, _options);
        return pages;
    }

    public void Convert(string fileName, Action<PageLayout[]> assert)
    {
        PageLayout[] pages = this.Convert(fileName);
        assert(pages);
    }
}
