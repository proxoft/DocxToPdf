using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Pages;
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
            return (PageLayout.None, ProcessingInfo.Done);
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

    public static (PageLayout, UpdateInfo) UpdatePage(
        this PageLayout pageLayout,
        Section[] sections,
        SectionLayout previousPageSectionLayout,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        (PageContentLayout content, UpdateInfo updateInfo) = pageLayout.PageContent.UpdatePageContent(sections, previousPageSectionLayout, fieldVariables, services);

        PageLayout updatedPage = pageLayout with
        {
            PageContent = content,
        };

        return (updatedPage, updateInfo);
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

        bool isFirstSection = true;

        foreach (Section section in sections)
        {
            if(section.ShouldRequestNewPage(isFirstSection, previousPageContent.Sections.LastOrDefault(SectionLayout.Empty)))
            {
                pageProcessingInfo = ProcessingInfo.NewPageRequired;
                break;
            }

            isFirstSection = false;
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
                sectionLayout.Offset(new Position(0, yOffset))
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

    private static (PageContentLayout, UpdateInfo) UpdatePageContent(
        this PageContentLayout pageContent,
        Section[] sections,
        SectionLayout previousPageSectionLayout,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = pageContent.BoundingBox.Size;
        SectionLayout[] sectionLayouts = [];

        UpdateInfo lastUpdateInfo = UpdateInfo.Done;

        SectionLayout lastSectionLayout = previousPageSectionLayout;

        float yOffset = 0;
        foreach (SectionLayout sectionLayout in pageContent.Sections)
        {
            Section section = sections.Single(s => s.Id == sectionLayout.ModelId);
            (SectionLayout updatedLayout, UpdateInfo updateInfo) = sectionLayout.Update(section, lastSectionLayout, remainingArea, fieldVariables, services);
            sectionLayouts = [.. sectionLayouts, updatedLayout.Offset(new Position(0, yOffset))];
            remainingArea = remainingArea.DecreaseHeight(updatedLayout.BoundingBox.Height);
            yOffset += updatedLayout.BoundingBox.Height;
            lastSectionLayout = updatedLayout;
            lastUpdateInfo = updateInfo;
            if (lastUpdateInfo is UpdateInfo.ReconstructRequired)
            {
                break;
            }
        }

        PageContentLayout updatedContent = pageContent with
        {
            Sections = sectionLayouts
        };

        return (updatedContent, lastUpdateInfo);
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
        PageLayout page = new(boundingBox, pageContent, pageConfiguration.Orientation);
        return page;
    }

    private static Rectangle CalculatePageBoundingBox(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size);

    private static Rectangle CalculatePageDrawingArea(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size)
            // .CropFromLeft(pageConfiguration.Margin.Left)
            .CropFromTop(pageConfiguration.Margin.Top)
            // .CropFromRight(pageConfiguration.Margin.Right)
            .CropFromBottom(pageConfiguration.Margin.Bottom);
}


file static class PageLayoutOperators
{
    public static Section[] Unprocessed(this Section[] sections, PageLayout lastPage) =>
        lastPage.PageContent.Sections.Length == 0
            ? sections
            : [.. sections.FilterProcessed(lastPage.PageContent.Sections)];

    public static bool ShouldRequestNewPage(this Section section, bool isOnPageFirst, SectionLayout previousSectionLayout) =>
        section.Properties.StartOnNextPage
            && !isOnPageFirst
            && previousSectionLayout.ModelId != section.Id;

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
