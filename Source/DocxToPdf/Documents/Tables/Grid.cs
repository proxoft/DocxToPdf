namespace Proxoft.DocxToPdf.Documents.Tables;

internal record Grid(float[] ColumnWidths, GridRow[] RowHeights);

internal record GridRow(float Height, HeightRule HeightRule);

internal enum HeightRule
{
    Auto,
    Exact,
    AtLeast
}