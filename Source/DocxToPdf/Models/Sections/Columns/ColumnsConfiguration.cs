using System.Linq;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Sections.Columns;

internal class ColumnsConfiguration
{
    private readonly ColumnConfig[] _columns;

    public ColumnsConfiguration(ColumnConfig[] columns)
    {
        _columns = columns;
        this.ColumnsCount = new PageColumn(_columns.Length);
    }

    public PageColumn ColumnsCount { get; }

    public HorizontalSpace CalculateColumnSpace(int columnIndex)
    {
        double xOffset = this.ColumnOffset(columnIndex);
        double width = this.ColumnWidth(columnIndex);

        return new HorizontalSpace(xOffset, width);
    }

    private double ColumnOffset(int columnIndex)
    {
        int columnConfigIndex = columnIndex % _columns.Length;

        double result = _columns
            .Take(columnConfigIndex)
            .Aggregate(0.0, (acc, column) =>
            {
                return acc + column.Width + column.Space;
            });

        return result;
    }

    private double ColumnWidth(int columnIndex)
    {
        int columnConfigIndex = columnIndex % _columns.Length;
        return _columns[columnConfigIndex].Width;
    }
}
