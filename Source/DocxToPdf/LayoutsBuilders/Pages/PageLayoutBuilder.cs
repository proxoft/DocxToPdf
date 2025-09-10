using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

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

        (Rectangle boundingBox, Orientation orientation) = unprocessed[0].GetPageProperties();

        (PageContentLayout pageContent, ProcessingInfo processingInfo) = unprocessed.CreatePageContent(
            boundingBox,
            previousPage.PageContent,
            fieldVariables,
            services
        );

        PageLayout page = pageContent.CreatePage(boundingBox, orientation);
        return (
            page,
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
}

file static class Functions
{
    //public static PageLayout CreateNewPage(this Section section) =>
    //    section.Properties.PageConfiguration.CreateNewPage();

    public static (Rectangle, Orientation) GetPageProperties(this Section section) =>
        (section.Properties.PageConfiguration.CalculatePageBoundingBox(), section.Properties.PageConfiguration.Orientation);

    public static Rectangle CalculatePageDrawingArea(this Section section) =>
        section.Properties.PageConfiguration.CalculatePageDrawingArea();

    public static PageLayout CreatePage(this PageContentLayout content, Rectangle pageBoundingBox, Orientation orientation) =>
        new(pageBoundingBox, content, orientation);

    private static Rectangle CalculatePageBoundingBox(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size);

    private static Rectangle CalculatePageDrawingArea(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size)
            .CropFromTop(pageConfiguration.Margin.Header)
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
