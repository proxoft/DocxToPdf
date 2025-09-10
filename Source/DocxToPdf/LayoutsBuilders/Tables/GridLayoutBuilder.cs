using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class GridLayoutBuilder
{
    public static GridLayout InitializeGridLayout(this Table table) =>
        table.Grid.InitializeGridLayout(table.Id);

    private static GridLayout InitializeGridLayout(this Grid grid, ModelId modelId) =>
        new(
            modelId,
            [..grid.ColumnWidths],
            []
        );

    public static GridLayout JustifyGridRows(
        this GridLayout gridLayout,
        ModelId modelId,
        Size cellSize,
        GridPosition gridPosition,
        Grid grid) =>
        gridLayout
            .EnsureRows(modelId, gridPosition, grid)
            .JustifyGridRows(cellSize, gridPosition);

    private static GridLayout JustifyGridRows(
        this GridLayout gridLayout,
        Size cellSize,
        GridPosition gridPosition)
    {
        float toDistribute = cellSize.Height - gridLayout.CalculateCellAvailableHeight(gridPosition);
        if (toDistribute <= 0)
        {
            return gridLayout;
        }

        float[] ratios = gridLayout.Rows.CalculateRowDistributionRatio(gridPosition);
        RowLayout[] rows = [
            ..gridLayout.Rows
                .Zip(ratios, (row, ratio) => (row, ratio))
                .Select(data => new RowLayout(
                    data.row.Row,
                    data.row.Height + toDistribute * data.ratio,
                    data.row.Rule)
                )
        ];

        return gridLayout with
        {
            Rows = rows
        };
    }

    private static float[] CalculateRowDistributionRatio(this RowLayout[] rows, GridPosition gridPosition)
    {
        int resizableRows = rows.Where(r => gridPosition.ContainsRowIndex(r.Row)).Count() - rows.FixedRowsCount(gridPosition);

        return [
            ..rows
                .Select(row =>
                    row.Rule == HeightRule.Exact || !gridPosition.ContainsRowIndex(row.Row)
                        ? 0
                        : 1f / resizableRows
                )
        ];
    }

    private static int FixedRowsCount(this RowLayout[] rows, GridPosition gridPosition) =>
        rows
            .Where(r => gridPosition.ContainsRowIndex(r.Row) && r.Rule == HeightRule.Exact)
            .Count();

    private static GridLayout EnsureRows(
        this GridLayout gridLayout,
        ModelId modelId,
        GridPosition gridPosition,
        Grid grid)
    {
        GridLayout layout = gridLayout;
        for (int ri = gridPosition.Row; ri < gridPosition.Row + gridPosition.RowSpan; ri++)
        {
            if(layout.Rows.Any(r => r.Row == ri))
            {
                continue; // row already exists
            }

            RowLayout row = new(ri, grid.RowHeights[ri].Height, grid.RowHeights[ri].HeightRule);
            layout = new GridLayout(
                modelId,
                layout.Columns,
                [..layout.Rows, row]
            );
        }

        return layout;
    }
}
