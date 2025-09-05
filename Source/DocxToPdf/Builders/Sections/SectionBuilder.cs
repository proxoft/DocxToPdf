using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Builders.Footers;
using Proxoft.DocxToPdf.Builders.Headers;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Common;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.HeadersFooters;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Footers;
using Proxoft.DocxToPdf.Documents.Headers;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Builders.Sections;

internal static class SectionBuilder
{
    public static Section[] ToSections(this Word.Body body, BuilderServices services) =>
        body.CreateSections(services);
}

file record SectionData(
    Word.SectionProperties SectionProperties,
    OpenXml.OpenXmlCompositeElement[] Elements,
    HeaderFooterConfiguration HeaderFooterConfiguration
);

file static class Operators
{
    public static Section[] CreateSections(this Word.Body body, BuilderServices services) =>
        [..body
            .SplitToSectionData(services)
            .Select(data => data.CreateSection(services))
        ];

    private static IEnumerable<SectionData> SplitToSectionData(this Word.Body body, BuilderServices services)
    {
        List<OpenXml.OpenXmlCompositeElement> sectionElements = [];

        foreach (OpenXml.OpenXmlCompositeElement child in body.ParagraphsAndTables())
        {
            sectionElements.Add(child);

            if (child is not Word.Paragraph p)
            {
                continue;
            }

            Word.SectionProperties? secProps = p.GetSectionProperties();
            if (secProps is null)
            {
                continue;
            }

            HeaderFooterConfiguration hfc = secProps.CreateHeaderFooterConfiguration(services);
            yield return new SectionData(secProps, [.. sectionElements], hfc);
            sectionElements.Clear();
        }

        Word.SectionProperties wordSectionProperties = body
           .ChildsOfType<Word.SectionProperties>()
           .Single();

        HeaderFooterConfiguration headerFooterConfiguration = wordSectionProperties.CreateHeaderFooterConfiguration(services);
        yield return new SectionData(wordSectionProperties, [.. sectionElements], headerFooterConfiguration);
    }

    private static Section CreateSection(this SectionData data, BuilderServices services)
    {
        SectionProperties props = data.SectionProperties.ToSectionProperties();
        Model[] elements = [..data.Elements.Select(e => e.ToParagraphOrTable(services))];
        return new Section(services.IdFactory.NextSectionId(), props, elements, data.HeaderFooterConfiguration);
    }

    private static SectionProperties ToSectionProperties(this Word.SectionProperties properties)
    {
        PageConfiguration pageConfiguration = properties.GetPageConfiguration();
        ColumnConfig[] columns = properties.CreateColumnConfigs(pageConfiguration);

        OpenXml.EnumValue<Word.SectionMarkValues> sectionMark = properties.ChildsOfType<Word.SectionType>().SingleOrDefault()?.Val ?? Word.SectionMarkValues.NextPage;
        bool startOnNextPage = sectionMark == Word.SectionMarkValues.NextPage;
        return new SectionProperties(pageConfiguration, columns, startOnNextPage);
    }

    private static PageConfiguration GetPageConfiguration(
        this Word.SectionProperties sectionProperties)
    {
        Word.PageSize pageSize = sectionProperties.ChildsOfType<Word.PageSize>().Single();
        float w = pageSize.Width.DxaToPoint();
        float h = pageSize.Height.DxaToPoint();

        Word.PageMargin pageMargin = sectionProperties.ChildsOfType<Word.PageMargin>().Single();

        PageMargin margin = new(
            pageMargin.Top.DxaToPoint(),
            pageMargin.Right.DxaToPoint(),
            pageMargin.Bottom.DxaToPoint(),
            pageMargin.Left.DxaToPoint(),
            pageMargin.Header.DxaToPoint(),
            pageMargin.Footer.DxaToPoint());

        Orientation orientation = (pageSize.Orient?.Value ?? Word.PageOrientationValues.Portrait) == Word.PageOrientationValues.Portrait
            ? Orientation.Portrait
            : Orientation.Landscape;

        return new PageConfiguration(margin, new Size(w, h), orientation);
    }

    private static ColumnConfig[] CreateColumnConfigs(
        this Word.SectionProperties sectionProperties,
        PageConfiguration pageConfiguration)
    {
        Word.Columns? columns = sectionProperties
            .ChildsOfType<Word.Columns>()
            .SingleOrDefault();

        float totalColumnsWidth = pageConfiguration.Size.Width - pageConfiguration.Margin.HorizontalMargins();
        int columnsCount = columns?.ColumnCount?.Value ?? 1;
        if (columnsCount == 1)
        {
            return [new ColumnConfig(totalColumnsWidth, 0)];
        }

        ColumnConfig[] cols = columns!.EqualWidth.IsOn(true)
            ? columns.CreatEqualWidthColumns(totalColumnsWidth)
            : columns.CreateUniqueColumns();

        return cols;
    }

    private static ColumnConfig[] CreatEqualWidthColumns(this Word.Columns columns, float totatWidth)
    {
        int columnsCount = columns.ColumnCount?.Value ?? 1;

        float space = columns.Space.ToPoint();
        float columnWidth = (totatWidth - space * (columnsCount - 1)) / columnsCount;

        return [
            ..Enumerable.Range(0, columnsCount)
                    .Select(i =>
                    {
                        float s = i == columnsCount - 1
                            ? 0
                            : space;
                        return new ColumnConfig(columnWidth, s);
                    })
        ];
    }

    private static ColumnConfig[] CreateUniqueColumns(this Word.Columns columns) =>
        [ ..columns
                .ChildsOfType<Word.Column>()
                .Select(col =>
                {
                    float cw = col.Width.ToPoint();
                    float space = col.Space.ToPoint();
                    return new ColumnConfig(cw, space);
                })
        ];

    private static HeaderFooterConfiguration CreateHeaderFooterConfiguration(
        this Word.SectionProperties sectionProperties,
        BuilderServices services
        )
    {
        bool hasTitlePage = sectionProperties
            .ChildsOfType<Word.TitlePage>()
            .SingleOrDefault()
            .IsOn(ifOnOffTypeNull: false, ifOnOffValueNull: true);

        Dictionary<PageNumberType, Header> headers = sectionProperties.CreateHeaders(services);
        Dictionary<PageNumberType, Footer> footers = sectionProperties.CreateFooters(services);

        return new HeaderFooterConfiguration(
            HasTitlePage: hasTitlePage,
            UseEvenOddHeader: services.HeaderFooterAccessor.UseEvenOddHeadersAndFooters(),
            headers,
            footers
        );
    }

    private static Dictionary<PageNumberType, Header> CreateHeaders(
        this Word.SectionProperties sectionProperties,
        BuilderServices services)
    {
        Dictionary<PageNumberType, Header> headers = sectionProperties
                .ChildsOfType<Word.HeaderReference>()
                .Where(hr => hr.Id is not null && hr.Type is not null)
                .Select(hr => (hr.Id, type: hr.Type!.ToHeaderFooterType()))
                .Select(hr => (header: services.HeaderFooterAccessor.FindHeader(hr.Id!), hr.type))
                .Where(h => h.header is not null)
                .Select(h => (header: h.header.Create(services), h.type))
                .ToDictionary(d => d.type, d => d.header);

        return headers;
    }

    private static Dictionary<PageNumberType, Footer> CreateFooters(
        this Word.SectionProperties sectionProperties,
        BuilderServices services)
    {
        Dictionary<PageNumberType, Footer> footers = sectionProperties
                .ChildsOfType<Word.FooterReference>()
                .Where(fr => fr.Id is not null && fr.Type is not null)
                .Select(fr => (fr.Id, type: fr.Type!.ToHeaderFooterType()))
                .Select(fr => (footer: services.HeaderFooterAccessor.FindFooter(fr.Id!), fr.type))
                .Where(f => f.footer is not null)
                .Select(f => (footer: f.footer.Create(services), f.type))
                .ToDictionary(d => d.type, d => d.footer);

        return footers;
    }
}