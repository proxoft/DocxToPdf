using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Footers;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Footers;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Tables;

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
        Size footerArea = new(
            pageAreaSize.Width - section.Properties.PageConfiguration.Margin.HorizontalMargins(),
            section.Properties.PageConfiguration.Margin.Bottom - section.Properties.PageConfiguration.Margin.Footer
        );

        FooterLayout footerLayout = footer.CreateLayout(footerArea, fieldVariables, services)
            .Offset(new Position(section.Properties.PageConfiguration.Margin.Left, pageAreaSize.Height - section.Properties.PageConfiguration.Margin.Bottom));

        return footerLayout;
    }

    private static FooterLayout CreateLayout(
        this Footer footer,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = availableArea;
        Layout[] layouts = [];

        float yOffset = 0;
        foreach (Model model in footer.ParagraphsOrTables)
        {
            (Layout layout, ProcessingInfo processingInfo) result = model switch
            {
                Paragraph p => p.CreateLayout(
                    ParagraphLayout.Empty,
                    remainingArea,
                    fieldVariables,
                    services),
                Table t => t.CreateTableLayout(
                    TableLayout.Empty,
                    remainingArea,
                    fieldVariables,
                    services),
                _ => (NoLayout.Instance, ProcessingInfo.Done)
            };

            if (result.layout.IsNotEmpty())
            {
                layouts = [.. layouts, result.layout.Offset(new Position(0, yOffset))];
            }

            if (result.processingInfo != ProcessingInfo.Done)
            {
                break;
            }

            yOffset += result.layout.BoundingBox.Height;
            remainingArea = remainingArea
                .DecreaseHeight(result.layout.BoundingBox.Height);
        }

        Rectangle bb = layouts
            .CalculateBoundingBox(availableArea);

        FooterLayout footerLayout = new(footer.Id, layouts, bb, Borders.None);
        return footerLayout;
    }
}

file static class Operators
{
    public static Footer FindFooter(this Section section, FieldVariables fieldVariables) =>
        section.HeaderFooterConfiguration.FindFooter(fieldVariables);

    private static Footer FindFooter(this HeaderFooterConfiguration configuration, FieldVariables fieldVariables) =>
        configuration.Footers.FindForPage(
            fieldVariables.CurrentPage,
            hasTitlePage: configuration.HasTitlePage,
            useEvenOdd: configuration.UseEvenOddHeader,
            Footer.None);
}