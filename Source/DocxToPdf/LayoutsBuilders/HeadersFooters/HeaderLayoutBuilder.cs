using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Headers;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Headers;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.LayoutsBuilders.HeadersFooters;

internal static class HeaderLayoutBuilder
{
    public static HeaderLayout CreateHeaderLayout(
        this Section section,
        Size pageAreaSize,
        FieldVariables fieldVariables,
        ILayoutServices services)
    {
        Header header = section.FindHeader(fieldVariables);
        Size headerArea = section.CalculateHeaderAvailableAreaSize(pageAreaSize);

        (Layout[] layouts, _) = header.ParagraphsOrTables.CreateParagraphAndTableLayouts(
            headerArea,
            NoLayout.Instance,
            fieldVariables, services
        );

        Rectangle bb = layouts
            .CalculateBoundingBox(headerArea);

        HeaderLayout headerLayout = new(header.Id, layouts, bb, Borders.None);
        Position offset = new(section.Properties.PageConfiguration.Margin.Left, section.Properties.PageConfiguration.Margin.Header);

        return headerLayout
            .Offset(offset);
    }

    public static HeaderLayout Update(
        this HeaderLayout headerLayout,
        Section section,
        Size pageAreaSize,
        FieldVariables fieldVariables,
        ILayoutServices services)
    {
        Header header = section.FindHeader(fieldVariables);
        Size headerArea = section.CalculateHeaderAvailableAreaSize(pageAreaSize);

        (Layout[] updatedLayouts, _) = headerLayout.ParagraphsAndTables.UpdateParagraphAndTableLayouts(
            header.ParagraphsOrTables,
            headerArea,
            NoLayout.Instance,
            fieldVariables,
            services);

        Rectangle bb = updatedLayouts
            .CalculateBoundingBox(headerArea);

        HeaderLayout updatedHeaderLayout = new(header.Id, updatedLayouts, bb, Borders.None);
        Position offset = new(section.Properties.PageConfiguration.Margin.Left, section.Properties.PageConfiguration.Margin.Header);
        return updatedHeaderLayout
            .Offset(offset);
    }
}

file static class Operators
{
    public static Size CalculateHeaderAvailableAreaSize(this Section section, Size pageAreaSize) =>
        new(
            pageAreaSize.Width - section.Properties.PageConfiguration.Margin.HorizontalMargins(),
            section.Properties.PageConfiguration.Margin.Top - section.Properties.PageConfiguration.Margin.Header
        );

    public static Header FindHeader(this Section section, FieldVariables fieldVariables) =>
        section.HeaderFooterConfiguration.FindHeader(fieldVariables);

    private static Header FindHeader(this HeaderFooterConfiguration configuration, FieldVariables fieldVariables) =>
        configuration.Headers.FindForPage(
            fieldVariables.CurrentPage,
            hasTitlePage: configuration.HasTitlePage,
            useEvenOdd: configuration.UseEvenOddHeader,
            Header.None);
}
