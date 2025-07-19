using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Common;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.Builders.Sections;

internal static class SectionBuilder
{
    public static Section[] ToSections(this Word.Body body, BuilderServices services) =>
        body.CreateSections(services);
}

file record SectionData(Word.SectionProperties SectionProperties, OpenXml.OpenXmlCompositeElement[] Elements);

file static class Operators
{
    public static Section[] CreateSections(this Word.Body body, BuilderServices services) =>
        [..body
            .SplitToSections()
            .Select(data => data.CreateSection(services))
        ];

    private static IEnumerable<SectionData> SplitToSections(this Word.Body body)
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

            yield return new SectionData(secProps, [.. sectionElements]);
            sectionElements.Clear();
        }

        Word.SectionProperties wordSectionProperties = body
           .ChildsOfType<Word.SectionProperties>()
           .Single();

        yield return new SectionData(wordSectionProperties, [.. sectionElements]);
    }

    private static Section CreateSection(this SectionData data, BuilderServices services)
    {
        SectionProperties props = data.SectionProperties.ToSectionProperties();
        Model[] elements = [..data.Elements.Select(e => e.ToParagraphOrTable(services))]; 
        return new Section(services.IdFactory.NextSectionId(), props, elements);
    }

    private static SectionProperties ToSectionProperties(this Word.SectionProperties properties)
    {
        PageConfiguration pageConfiguration = properties.GetPageConfiguration();
        ColumnConfig[] columns = properties.CreateColumnConfigs(pageConfiguration);
        return new SectionProperties(pageConfiguration, columns);
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
}