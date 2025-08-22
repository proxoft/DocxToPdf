using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Pages;

internal static class PageLayoutBuilder
{
    public static (PageLayout, ProcessingInfo) CreatePage(
        this Section[] sections,
        PageLayout previousPage,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Section[] unprocessed = sections.Unprocessed(previousPage);
        if(unprocessed.Length == 0)
        {
            return (PageLayout.None, ProcessingInfo.Ignore);
        }

        PageLayout page = unprocessed[0].CreateNewPage();
        (PageContentLayout pageContent, ProcessingInfo processingInfo) = unprocessed.CreatePageContent(
            page.PageContent.BoundingBox,
            previousPage.PageContent,
            fieldVariables,
            services
        );

        return (
            page with
            {
                PageContent = pageContent,
            },
            processingInfo
        );
    }

    public static (PageLayout, ProcessingInfo) UpdatePage(
        this PageLayout pageLayout,
        Section[] sections,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        (PageContentLayout content, ProcessingInfo processingInfo) = pageLayout.PageContent.UpdatePageContent(sections, fieldVariables, services);

        PageLayout updatedPage = pageLayout with
        {
            PageContent = content,
        };

        return (updatedPage, processingInfo);
    }

    private static (PageContentLayout pageContent, ProcessingInfo processingInfo) CreatePageContent(
        this Section[] sections,
        Rectangle drawingArea,
        PageContentLayout previousPageContent,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        SectionLayout[] sectionLayouts = [];
        Size remainingArea = drawingArea.Size;
        ProcessingInfo pageProcessingInfo = ProcessingInfo.Done;
        float yOffset = 0;

        foreach (Section section in sections)
        {
            SectionLayout lastSectionLayout = previousPageContent.Sections
                .Where(l => l.ModelId == section.Id)
                .LastOrDefault(SectionLayout.Empty);

            (SectionLayout sectionLayout, ProcessingInfo processingInfo) = section.CreateLayout(
                lastSectionLayout,
                remainingArea,
                fieldVariables,
                services
            );

            sectionLayouts = [
                .. sectionLayouts,
                sectionLayout.SetOffset(new Position(0, yOffset))
            ];

            yOffset += sectionLayout.BoundingBox.Height;

            if (processingInfo is ProcessingInfo.NewPageRequired
                or ProcessingInfo.RequestDrawingArea
                or ProcessingInfo.IgnoreAndRequestDrawingArea)
            {
                pageProcessingInfo = ProcessingInfo.NewPageRequired;
                break;
            }
        }
        PageContentLayout pageContent = new(sectionLayouts, drawingArea);
        return (pageContent, pageProcessingInfo);
    }

    private static (PageContentLayout, ProcessingInfo) UpdatePageContent(
        this PageContentLayout pageContent,
        Section[] sections,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = pageContent.BoundingBox.Size;
        SectionLayout[] sectionLayouts = [];
        ProcessingInfo pageProcessingInfo = ProcessingInfo.Done;

        SectionLayout lastSectionLayout = SectionLayout.Empty;

        float yOffset = 0;
        foreach (SectionLayout sectionLayout in pageContent.Sections)
        {
            Section section = sections.Single(s => s.Id == sectionLayout.ModelId);
            (SectionLayout updatedLayout, ProcessingInfo processingInfo) = sectionLayout.Update(section, lastSectionLayout, remainingArea, fieldVariables, services);
            sectionLayouts = [.. sectionLayouts, updatedLayout.SetOffset(new Position(0, yOffset))];
            remainingArea = remainingArea.DecreaseHeight(updatedLayout.BoundingBox.Height);
            yOffset += updatedLayout.BoundingBox.Height;
            lastSectionLayout = updatedLayout;
            if (processingInfo is ProcessingInfo.ReconstructRequired)
            {
                pageProcessingInfo = processingInfo;
                break;
            }
        }

        PageContentLayout updatedContent = pageContent with
        {
            Sections = sectionLayouts
        };

        return (updatedContent, pageProcessingInfo);
    }
}

file static class Functions
{
    public static PageLayout CreateNewPage(this Section section) =>
        section.Properties.PageConfiguration.CreateNewPage();

    private static PageLayout CreateNewPage(this PageConfiguration pageConfiguration)
    {
        Rectangle boundingBox = pageConfiguration.CalculatePageBoundingBox();
        Rectangle contentRegion = pageConfiguration.CalculatePageDrawingArea();
        PageContentLayout pageContent = new([], contentRegion);
        PageLayout page = new(boundingBox, pageContent, pageConfiguration);
        return page;
    }

    private static Rectangle CalculatePageBoundingBox(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size);

    private static Rectangle CalculatePageDrawingArea(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size)
            .CropFromLeft(pageConfiguration.Margin.Left)
            .CropFromTop(pageConfiguration.Margin.Top)
            .CropFromRight(pageConfiguration.Margin.Right)
            .CropFromBottom(pageConfiguration.Margin.Bottom);
}


file static class PageLayoutOperators
{
    public static Section[] Unprocessed(this Section[] sections, PageLayout lastPage) =>
        lastPage.PageContent.Sections.Length == 0
            ? sections
            : [.. sections.FilterProcessed(lastPage.PageContent.Sections)];

    private static IEnumerable<Section> FilterProcessed(this Section[] sections, SectionLayout[] sectionLayouts)
    {
        SectionLayout lastLayout = sectionLayouts.Last();

        int skipLast = lastLayout.Partition.IsFinished()
            ? 1
            : 0;

        return sections
            .SkipWhile(s => s.Id != lastLayout.ModelId)
            .Skip(skipLast);
    }
}
