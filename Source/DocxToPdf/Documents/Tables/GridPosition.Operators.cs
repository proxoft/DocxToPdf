namespace Proxoft.DocxToPdf.Documents.Tables;

internal static class GridPositionOperators
{
    public static int RightColumn(this GridPosition gridPosition) =>
        gridPosition.Column + gridPosition.ColumnSpan - 1;

    public static int BottomRow(this GridPosition gridPosition) =>
        gridPosition.Row + gridPosition.RowSpan - 1;

    public static bool ContainsColumnIndex(this GridPosition gridPosition, int index) =>
        gridPosition.Column <= index && index < (gridPosition.Column + gridPosition.ColumnSpan);

    public static bool ContainsRowIndex(this GridPosition gridPosition, int index) =>
        gridPosition.Row <= index && index < (gridPosition.Row + gridPosition.RowSpan);
}