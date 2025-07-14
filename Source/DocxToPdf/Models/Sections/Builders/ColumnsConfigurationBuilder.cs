using System.Linq;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Extensions.Units;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Sections.Columns;

namespace Proxoft.DocxToPdf.Models.Sections.Builders;

internal static class ColumnsConfigurationBuilder
{
    public static ColumnsConfiguration CreateColumnsConfiguration(
        this Word.SectionProperties sectionProperties,
        PageConfiguration pageConfiguration,
        PageMargin pageMargin)
    {
        ColumnConfig[] columns = sectionProperties.GetSectionColumnConfigs(pageConfiguration, pageMargin);
        return new ColumnsConfiguration(columns);
    }

    private static ColumnConfig[] GetSectionColumnConfigs(
        this Word.SectionProperties wordSectionProperties,
        PageConfiguration page,
        PageMargin pageMargin)
    {
        Word.Columns? columns = wordSectionProperties
            .ChildsOfType<Word.Columns>()
            .SingleOrDefault();

        double totalColumnsWidth = page.Width - pageMargin.HorizontalMargins;
        int columnsCount = columns?.ColumnCount?.Value ?? 1;
        if (columnsCount == 1)
        {
            return [ new ColumnConfig(totalColumnsWidth, 0) ];
        }

        if (columns!.EqualWidth.IsOn(true)) // columns cannot be null
        {
            double space = columns.Space.ToPoint();
            double columnWidth = (totalColumnsWidth - space * (columnsCount - 1)) / columnsCount;

            return [
                ..Enumerable.Range(0, columnsCount)
                .Select(i =>
                {
                    double s = i == columnsCount - 1
                        ? 0
                        : space;
                    return new ColumnConfig(columnWidth, s);
                })
            ];
        }

        ColumnConfig[] cols = [
            ..columns
                .ChildsOfType<Word.Column>()
                .Select(col =>
                {
                    double cw = col.Width.ToPoint();
                    double space = col.Space.ToPoint();
                    return new ColumnConfig(cw, space);
                })
        ];

        return cols;
    }
}
