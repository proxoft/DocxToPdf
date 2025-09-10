using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Footers;
using Proxoft.DocxToPdf.Layouts.Headers;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.HeadersFooters;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Pages;

internal static class PageContentLayoutBuilder
{
    public static (PageContentLayout pageContent, ProcessingInfo processingInfo) CreatePageContent(
        this Section[] sections,
        Rectangle pageArea,
        PageContentLayout previousPageContent,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        if (sections.Length == 0)
        {
            return (PageContentLayout.Empty, ProcessingInfo.Done);
        }

        SectionLayout[] sectionLayouts = [];
        ProcessingInfo pageProcessingInfo = ProcessingInfo.Done;

        Size remainingArea = pageArea.Size
            .DecreaseHeight(sections[0].Properties.PageConfiguration.Margin.Top)
            .DecreaseHeight(sections[0].Properties.PageConfiguration.Margin.Bottom)
            ;

        float yOffset = sections[0].Properties.PageConfiguration.Margin.Top;

        HeaderLayout headerLayout = sections[0].CreateHeaderLayout(pageArea.Size, fieldVariables, services);
        FooterLayout footerLayout = sections[0].CreateFooterLayout(pageArea.Size, fieldVariables, services);

        bool isFirstSection = true;
        foreach (Section section in sections)
        {
            if (section.ShouldRequestNewPage(isFirstSection, previousPageContent.Sections.LastOrDefault(SectionLayout.Empty)))
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

        PageContentLayout pageContent = new(headerLayout, sectionLayouts, footerLayout, pageArea);
        return (pageContent, pageProcessingInfo);
    }

    public static (PageContentLayout, UpdateInfo) UpdatePageContent(
        this PageContentLayout pageContent,
        Section[] sections,
        SectionLayout previousPageSectionLayout,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        SectionLayout[] sectionLayouts = [];
        UpdateInfo lastUpdateInfo = UpdateInfo.Done;
        SectionLayout lastSectionLayout = previousPageSectionLayout;

        Size remainingArea = pageContent.BoundingBox.Size
            .DecreaseHeight(sections[0].Properties.PageConfiguration.Margin.Top)
            .DecreaseHeight(sections[0].Properties.PageConfiguration.Margin.Bottom)
            ;
        float yOffset = sections[0].Properties.PageConfiguration.Margin.Top;

        Section footerHeaderSection = sections.Find<Section>(pageContent.Sections.First().ModelId);
        HeaderLayout header = pageContent.Header.Update(footerHeaderSection, pageContent.BoundingBox.Size, fieldVariables, services);
        FooterLayout footer = pageContent.Footer.Update(footerHeaderSection, pageContent.BoundingBox.Size, fieldVariables, services);

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
            Header = header,
            Sections = sectionLayouts,
            Footer = footer
        };

        return (updatedContent, lastUpdateInfo);
    }
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
