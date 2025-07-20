using System.Linq;
using Proxoft.DocxToPdf.Documents.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class GridOperators
{
    public static (float offset, float width) CalculateCellRegion(this Grid grid, GridPosition gridPosition)
    {
        float offset = grid.CalculateCellXOffset(gridPosition);
        float width = grid.CalculateCellWidth(gridPosition);
        return (offset, width);
    }

    private static float CalculateCellXOffset(this Grid grid, GridPosition gridPosition) =>
        grid.ColumnWidths
            .Take(gridPosition.Column - 1)
            .Sum();

    private static float CalculateCellWidth(this Grid grid, GridPosition gridPosition) =>
        grid.ColumnWidths
            .Skip(gridPosition.Column)
            .Take(gridPosition.ColumnSpan)
            .Sum();
}
