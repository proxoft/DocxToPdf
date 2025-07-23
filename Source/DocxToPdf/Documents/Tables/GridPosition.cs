namespace Proxoft.DocxToPdf.Documents.Tables;

internal record GridPosition(int Column, int ColumnSpan, int Row, int RowSpan);

internal static class GridPositionOperators
{
    public static bool ContainsColumnIndex(this GridPosition gridPosition, int index) =>
        gridPosition.Column <= index && index < (gridPosition.Column + gridPosition.ColumnSpan);

    public static bool ContainsRowIndex(this GridPosition gridPosition, int index) =>
        gridPosition.Row <= index && index < (gridPosition.Row + gridPosition.RowSpan);
}