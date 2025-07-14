using System;
using System.Diagnostics;
using Proxoft.DocxToPdf.Core.Pages;

namespace Proxoft.DocxToPdf.Models.Common;

[DebuggerDisplay("PN: {PageNumber}, {Column}/{_totalColumns}")]
internal class PagePosition(PageNumber pageNumber, PageColumn column, PageColumn totalColumns) : IEquatable<PagePosition>, IComparable<PagePosition>
{
    public static readonly PagePosition None = new(PageNumber.None, PageColumn.None, PageColumn.None);

    private readonly PageColumn _totalColumns = totalColumns;

    public PagePosition(PageNumber pageNumber) : this(pageNumber, PageColumn.First, PageColumn.One)
    {
    }

    public PageNumber PageNumber { get; } = pageNumber;

    public PageColumn Column { get; } = column;

    public int PageColumnIndex => this.Column - 1;

    public PagePosition Next()
    {
        if(_totalColumns <= 0)
        {
            throw new Exception("Total Columns is zero");
        }

        return this.Column == _totalColumns
            ? this.NewPage()
            : this.NextColumn();
    }

    public PagePosition NextPage(PageColumn column, PageColumn totalColumns)
        => new(this.PageNumber.Next(), column, totalColumns);

    public PagePosition SamePage(PageColumn column, PageColumn totalColumns)
        => new(this.PageNumber, column, totalColumns);

    private PagePosition NextColumn()
        => new(this.PageNumber, this.Column.Next(), _totalColumns);

    private PagePosition NewPage()
        => new(this.PageNumber.Next(), PageColumn.First, _totalColumns);

    public int CompareTo(PagePosition? other)
    {
        if(other is null)
        {
            return 1;
        }

        int pnSign = this.PageNumber.CompareTo(other.PageNumber);
        if (pnSign != 0)
        {
            return pnSign;
        }

        return this.Column.CompareTo(other.Column);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.PageNumber.GetHashCode(), this.Column.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as PagePosition);
    }

    public bool Equals(PagePosition? other)
    {
        return other is not null
            && other.PageNumber == this.PageNumber
            && other.Column == this.Column;
    }

    public static bool operator ==(PagePosition p1, PagePosition p2)
    {
        return p1.Equals(p2);
    }

    public static bool operator !=(PagePosition p1, PagePosition p2)
    {
        return !(p1 == p2);
    }
}
