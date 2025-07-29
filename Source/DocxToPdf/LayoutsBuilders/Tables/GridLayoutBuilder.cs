using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class GridLayoutBuilder
{
    public static GridLayout InitializeGridLayout(this Grid grid) =>
        new(
            [..grid.ColumnWidths],
            []
        );

    public static GridLayout JustifyGridRows(
        this GridLayout gridLayout,
        Rectangle cellBoundingBox,
        GridPosition gridPosition,
        Grid grid) =>
        gridLayout
            .EnsureRows(gridPosition, grid)
            .JustifyGridRows(cellBoundingBox, gridPosition);

    private static GridLayout JustifyGridRows(
        this GridLayout gridLayout,
        Rectangle cellBoundingBox,
        GridPosition gridPosition)
    {
        float toDistribute = cellBoundingBox.Height - gridLayout.CalculateCellAvailableHeight(gridPosition);
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

    //public static GridLayout InitializeGridLayout(this Grid grid, GridLayout previous) =>
    //    previous == GridLayout.Empty
    //        ? new GridLayout(
    //            [.. grid.ColumnWidths],
    //            [new RowLayout(0, grid.RowHeights.First().Height, grid.RowHeights.First().HeightRule)]
    //        )
    //        : previous;


    //public static GridLayout CreateGridLayout(this Grid grid) =>
    //    new(
    //        [..grid.ColumnWidths],
    //        [.. grid.RowHeights.Select((r, index) => new RowLayout(index, r.Height, r.HeightRule))]
    //    );

    //public static GridLayout JustifyGridRows(
    //    this GridLayout grid,
    //    Rectangle cellBoundingBox,
    //    GridPosition gridPosition)
    //{
    //    float toDistribute = cellBoundingBox.Height - grid.CalculateCellAvailableHeight(gridPosition);
    //    if(toDistribute <= 0)
    //    {
    //        return grid;
    //    }

    //    float[] ratios = grid.Rows.CalculateRowDistributionRatio(gridPosition);
    //    RowLayout[] rows = [
    //        ..grid.Rows
    //            .Zip(ratios, (row, ratio) => (row, ratio))
    //            .Select(data => new RowLayout(
    //                data.row.Height + toDistribute * data.ratio,
    //                data.row.Rule)
    //            )
    //    ];

    //    return grid with
    //    {
    //        Rows = rows
    //    };
    //}

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
        GridPosition gridPosition,
        Grid grid)
    {
        // create missing rows
        GridLayout layout = gridLayout;
        for (int ri = gridPosition.Row; ri < gridPosition.Row + gridPosition.RowSpan; ri++)
        {
            if(layout.Rows.Any(r => r.Row == ri))
            {
                continue; // row already exists
            }

            RowLayout row = new(ri, 0, grid.RowHeights[ri].HeightRule);
            layout = new GridLayout(
                layout.Columns,
                [..layout.Rows, row]
            );
        }

        return layout;
    }
}
