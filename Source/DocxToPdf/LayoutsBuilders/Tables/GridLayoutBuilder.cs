using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class GridLayoutBuilder
{
    public static GridLayout CreateGridLayout(this Grid grid) =>
        new(
            [..grid.ColumnWidths],
            [.. grid.RowHeights.Select(r => new RowLayout(r.Height, r.HeightRule))]
        );

    public static GridLayout JustifyGridRows(
        this GridLayout grid,
        Rectangle cellBoundingBox,
        GridPosition gridPosition)
    {
        float toDistribute = cellBoundingBox.Height - grid.CalculateCellAvailableHeight(gridPosition);
        if(toDistribute <= 0)
        {
            return grid;
        }

        float[] ratios = grid.Rows.CalculateRowDistributionRatio(gridPosition);
        RowLayout[] rows = [
            ..grid.Rows
                .Zip(ratios, (row, ratio) => (row, ratio))
                .Select(data => new RowLayout(
                    data.row.Height + toDistribute * data.ratio,
                    data.row.Rule)
                )
        ];

        return grid with
        {
            Rows = rows
        };
    }

    private static float[] CalculateRowDistributionRatio(this RowLayout[] rows, GridPosition gridPosition)
    {
        int resizableRows = rows.Where((r, i) => i.IsInGridPosition(gridPosition)).Count() - rows.FixedRowsCount(gridPosition);

        return [
            ..rows
                .Select((row, index) =>
                    row.Rule == HeightRule.Exact || !index.IsInGridPosition(gridPosition)
                        ? 0
                        : 1f / resizableRows
                )
        ];
    }

    private static int FixedRowsCount(this RowLayout[] rows, GridPosition gridPosition) =>
        rows
            .Skip(gridPosition.Row)
            .Take(gridPosition.RowSpan)
            .Where(r => r.Rule == HeightRule.Exact)
            .Count();

    private static bool IsInGridPosition(this int index, GridPosition gridPosition) =>
        gridPosition.Row <= index && index < (gridPosition.Row + gridPosition.RowSpan);
}
