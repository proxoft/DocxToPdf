using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Pages;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal class LayoutBuilder
{
    private readonly LayoutServices _layoutServices = new();

    public PageLayout[] CreatePages(DocumentModel document) =>
        document.Sections.Length == 0
            ? []
            : this.CreatePages(document.Sections);

    private PageLayout[] CreatePages(Section[] sections)
    {
        PageLayout[] pages = [];
        bool done = false;
        int minimalTotalPages = 1;
        int currentPageNumber = 1;

        PageLayout lastPage = PageLayout.None;
        while (!done)
        {
            FieldVariables variables = new(currentPageNumber, minimalTotalPages);
            (PageLayout page, ProcessingInfo processingInfo) = sections.CreatePage(lastPage, variables, _layoutServices);

            if (processingInfo != ProcessingInfo.Ignore)
            {
                pages = [.. pages, page];
            }

            if(processingInfo is ProcessingInfo.NewPageRequired or ProcessingInfo.RequestDrawingArea)
            {
                minimalTotalPages++;
                pages = this.UpdatePages(pages, sections, minimalTotalPages);
                currentPageNumber = pages.Length + 1;
            }

            lastPage = pages.Last();

            done = processingInfo is ProcessingInfo.Done or ProcessingInfo.Ignore;
        }

        return pages;
    }

    private PageLayout[] UpdatePages(PageLayout[] pages, Section[] sections, int totalPages)
    {
        PageLayout[] updatedPages = [];
        int currentPage = 0;
        foreach (PageLayout page in pages)
        {
            currentPage++;
            (PageLayout updated, ProcessingInfo processingInfo)  = page.UpdatePage(sections, new FieldVariables(currentPage, totalPages), _layoutServices);
            updatedPages = [.. updatedPages, updated];

            if(processingInfo != ProcessingInfo.Done)
            {
                break;
            }
        }

        return updatedPages;
    }
}