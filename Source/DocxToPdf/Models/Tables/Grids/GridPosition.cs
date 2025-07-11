using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Proxoft.DocxToPdf.Models.Tables.Grids;

[DebuggerDisplay("{Column}({ColumnSpan})-{Row}({RowSpan})")]
internal class GridPosition(
    int column,
    int columnSpan,
    int row,
    int rowSpan)
{
    private readonly int[] _rowIndeces = rowSpan > 0
            ? [.. Enumerable.Range(row, rowSpan)]
            : [];

    public int Column { get; } = column;

    public int ColumnSpan { get; } = columnSpan;

    public int Row { get; } = row;

    public int RowSpan { get; } = rowSpan;

    public IReadOnlyCollection<int> RowIndeces => _rowIndeces;

    public bool IsRowMergedCell => this.RowSpan <= 0;
}
