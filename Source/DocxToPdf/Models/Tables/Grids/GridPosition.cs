using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Proxoft.DocxToPdf.Models.Tables.Grids;

[DebuggerDisplay("{Column}({ColumnSpan})-{Row}({RowSpan})")]
internal class GridPosition
{
    private readonly int[] _rowIndeces;

    public GridPosition(
        int column,
        int columnSpan,
        int row,
        int rowSpan)
    {
        this.Row = row;
        this.Column = column;
        this.RowSpan = rowSpan;
        this.ColumnSpan = columnSpan;

        _rowIndeces = rowSpan > 0
            ? [.. Enumerable.Range(row, rowSpan)]
            : [];
    }

    public int Column { get; }
    public int ColumnSpan { get; }
    public int Row { get; }
    public int RowSpan { get; }
    public bool IsFirstVerticalCell => this.RowSpan > 0;
    public bool IsLastVerticalCell { get; }

    public IReadOnlyCollection<int> RowIndeces => _rowIndeces;

    public bool IsRowMergedCell => this.RowSpan <= 0;
}
