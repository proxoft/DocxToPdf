using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Headers;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Headers;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Tables;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Headers;

internal static class HeaderLayoutBuilder
{
    public static HeaderLayout CreateHeaderLayout(
        this Section section,
        Size pageAreaSize,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Header header = section.FindHeader(fieldVariables);
        Size headerArea = new(
            pageAreaSize.Width - section.Properties.PageConfiguration.Margin.HorizontalMargins(),
            section.Properties.PageConfiguration.Margin.Top - section.Properties.PageConfiguration.Margin.Header
        );

        HeaderLayout headerLayout = header.CreateLayout(headerArea, fieldVariables, services)
            .Offset(new Position(section.Properties.PageConfiguration.Margin.Left, section.Properties.PageConfiguration.Margin.Header));

        return headerLayout;
    }

    private static HeaderLayout CreateLayout(
        this Header header,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = availableArea;
        Layout[] layouts = [];

        float yOffset = 0;
        foreach (Model model in header.ParagraphsOrTables)
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
                layouts = [..layouts, result.layout.Offset(new Position(0, yOffset))];
            }

            if(result.processingInfo != ProcessingInfo.Done)
            {
                break;
            }

            yOffset += result.layout.BoundingBox.Height;
            remainingArea = remainingArea
                .DecreaseHeight(result.layout.BoundingBox.Height);
        }

        Rectangle bb = layouts
            .CalculateBoundingBox(availableArea);

        HeaderLayout headerLayout = new(header.Id, layouts, bb, Borders.None);
        return headerLayout;
    }
}

file static class Operators
{
    public static Header FindHeader(this Section section, FieldVariables fieldVariables) =>
        section.HeaderFooterConfiguration.FindHeader(fieldVariables);

    private static Header FindHeader(this HeaderFooterConfiguration configuration, FieldVariables fieldVariables)
    {
        if (configuration.Headers.Count == 0) return Header.None;
        if (fieldVariables.CurrentPage == 1) return configuration.Headers.FindFirstPageHeader();
        if (fieldVariables.CurrentPage % 2 == 0) return configuration.Headers.FindEvenPageHeader();
        return configuration.Headers.FindOddPageHeader();
    }

    private static Header FindFirstPageHeader(this Dictionary<HeaderFooterType, Header> headers)
    {
        if(headers.ContainsKey(HeaderFooterType.First)) return headers[HeaderFooterType.First];
        if(headers.ContainsKey(HeaderFooterType.Default)) return headers[HeaderFooterType.Default];
        return Header.None;
    }

    private static Header FindOddPageHeader(this Dictionary<HeaderFooterType, Header> headers)
    {
        if (headers.ContainsKey(HeaderFooterType.Default)) return headers[HeaderFooterType.Default];
        return Header.None;
    }

    private static Header FindEvenPageHeader(this Dictionary<HeaderFooterType, Header> headers)
    {
        if (headers.ContainsKey(HeaderFooterType.Even)) return headers[HeaderFooterType.Even];
        if (headers.ContainsKey(HeaderFooterType.Default)) return headers[HeaderFooterType.Default];
        return Header.None;
    }
}
