using System;

namespace Proxoft.DocxToPdf.Models.Common;

internal record PageColumn : IComparable<PageColumn>
{
    public static readonly PageColumn None = new(0);
    public static readonly PageColumn First = new(1);
    public static readonly PageColumn One = new(1);

    public PageColumn(int column)
    {
        this.Column = column;
    }

    public int Column { get; }

    public int CompareTo(PageColumn? other)
    {
        return this.Column - (other?.Column ?? 0);
    }

    public PageColumn Next() =>
        new(this.Column + 1);

    public static implicit operator int(PageColumn pageColumn) => pageColumn.Column;
}
