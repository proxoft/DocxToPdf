using System.Linq;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class PageLayoutBuilder
{
    public static (PageLayout[] updatedPages, SectionLayoutingResult sectionLayoutingResult) Update(
        this PageLayout[] pages,
        Section[] sections,
        SectionLayoutingResult lastLayoutingResult,
        LayoutServices services)
    {
        int totalPages = pages.Length;
        int currentPage = 1;

        PageLayout[] updatedPages = [];
        foreach (PageLayout page in pages)
        {
            FieldVariables fieldVariables = new(currentPage, totalPages);
            (PageLayout updatedPage, _) = Update(page, sections, fieldVariables, services);
            updatedPages = [.. updatedPages, updatedPage];
            currentPage++;
        }

        return (updatedPages, lastLayoutingResult);
    }

    private static (PageLayout page, SectionLayoutingResult sectionLayoutingResult) Update(
        PageLayout page,
        Section[] sections,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        SectionLayout[] newContent = [];
        SectionLayoutingResult sectionLayoutingResult = SectionLayoutingResult.None;
        foreach (SectionLayout sectionLayout in page.Content)
        {
            Section section = sections.First(s => s.Id == sectionLayout.ModelId);
            sectionLayoutingResult = sectionLayout.UpdateLayout(
                section,
                page.DrawingArea,
                fieldVariables,
                services
            );

            newContent = [.. newContent, sectionLayoutingResult.SectionLayout];
            // check for break condition
        }

        PageLayout updatedPage = page with
        {
            Content = newContent,
        };

        return (updatedPage, sectionLayoutingResult);
    }
}
