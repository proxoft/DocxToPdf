using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Footers;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Footers;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.HeadersFooters;

internal static class FooterLayoutBuilder
{
    public static FooterLayout CreateFooterLayout(
        this Section section,
        Size pageAreaSize,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Footer footer = section.FindFooter(fieldVariables);
        Size footerArea = section.CalculateFooterAvailableAreaSize(pageAreaSize);

        (Layout[] layouts, _) = footer.ParagraphsOrTables.CreateParagraphAndTableLayouts(
            footerArea,
            NoLayout.Instance,
            fieldVariables, services
        );

        Rectangle bb = layouts
            .CalculateBoundingBox(footerArea);

        FooterLayout footerLayout = new(footer.Id, layouts, bb, Borders.None);
        Position offset = new(
            section.Properties.PageConfiguration.Margin.Left,
            pageAreaSize.Height - section.Properties.PageConfiguration.Margin.Bottom
        );

        return footerLayout
            .Offset(offset);
    }

    public static FooterLayout Update(
        this FooterLayout footerLayout,
        Section section,
        Size pageAreaSize,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Footer footer = section.FindFooter(fieldVariables);
        Size footerArea = section.CalculateFooterAvailableAreaSize(pageAreaSize);

        (Layout[] updatedLayouts, _) = footerLayout.ParagraphsAndTables.UpdateParagraphAndTableLayouts(
            footer.ParagraphsOrTables,
            footerArea,
            NoLayout.Instance,
            fieldVariables,
            services);

        Rectangle bb = updatedLayouts
            .CalculateBoundingBox(footerArea);

        FooterLayout updatedFooterLayout = new(footer.Id, updatedLayouts, bb, Borders.None);
        Position offset = new(
            section.Properties.PageConfiguration.Margin.Left,
            pageAreaSize.Height - section.Properties.PageConfiguration.Margin.Bottom);

        // return footerLayout;
        return updatedFooterLayout
            .Offset(offset);
    }
}

file static class Operators
{
    public static Size CalculateFooterAvailableAreaSize(this Section section, Size pageAreaSize) =>
       new(
           pageAreaSize.Width - section.Properties.PageConfiguration.Margin.HorizontalMargins(),
            section.Properties.PageConfiguration.Margin.Bottom - section.Properties.PageConfiguration.Margin.Footer
       );

    public static Footer FindFooter(this Section section, FieldVariables fieldVariables) =>
        section.HeaderFooterConfiguration.FindFooter(fieldVariables);

    private static Footer FindFooter(this HeaderFooterConfiguration configuration, FieldVariables fieldVariables) =>
        configuration.Footers.FindForPage(
            fieldVariables.CurrentPage,
            hasTitlePage: configuration.HasTitlePage,
            useEvenOdd: configuration.UseEvenOddHeader,
            Footer.None);
}