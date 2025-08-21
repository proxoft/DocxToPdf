namespace Proxoft.DocxToPdf.Documents.Tables;

internal record GridPosition(int Column, int ColumnSpan, int Row, int RowSpan)
{
    public static readonly GridPosition None = new(0, 0, 0, 0);
}
