using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Pages;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal class LayoutBuilder
{
    private readonly ILayoutServices _layoutServices = LayoutServicesFactory.CreateServices();

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

            if (page != PageLayout.None)
            {
                pages = [.. pages, page];
            }

            if(processingInfo is ProcessingInfo.RequestDrawingArea
                or ProcessingInfo.NewPageRequired
                or ProcessingInfo.IgnoreAndRequestDrawingArea)
            {
                minimalTotalPages++;
                pages = this.UpdatePages(pages, sections, minimalTotalPages);
                currentPageNumber = pages.Length + 1;
            }

            lastPage = pages.Last();

            done = processingInfo is ProcessingInfo.Done;
        }

        return pages;
    }

    private PageLayout[] UpdatePages(PageLayout[] pages, Section[] sections, int totalPages)
    {
        PageLayout[] updatedPages = [];
        int currentPage = 0;
        foreach (PageLayout page in pages)
        {
            SectionLayout previousPageSectionLayout = currentPage == 0
                ? SectionLayout.Empty
                : updatedPages[currentPage - 1].PageContent.Sections.LastOrDefault(SectionLayout.Empty);

            currentPage++;
            (PageLayout updated, UpdateInfo updateInfo)  = page.UpdatePage(
                sections,
                previousPageSectionLayout,
                new FieldVariables(currentPage, totalPages),
                _layoutServices
            );
            updatedPages = [.. updatedPages, updated];

            if(updateInfo == UpdateInfo.ReconstructRequired)
            {
                break;
            }
        }

        return updatedPages;
    }
}