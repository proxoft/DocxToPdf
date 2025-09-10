using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class TableLayoutBuilder
{
    public static (TableLayout, ProcessingInfo) CreateTableLayout(
        this Table table,
        TableLayout previousLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        ILayoutServices services)
    {
        GridLayout gridLayout = table.InitializeGridLayout();
        Rectangle[] columnsAvailableArea = gridLayout.SplitToColumnAreas(availableArea);
        bool[] columnFinished = [.. columnsAvailableArea.Select(_ => false)];
        CellLayout[] cellLayouts = [];
        ProcessingInfo[] processingInfos = [];

        foreach (Cell cell in table.Cells.InLayoutingOrder().SkipFinished(previousLayout.Cells))
        {
            Rectangle cellAvailableArea = cell.CalculateCellAvailableArea(columnsAvailableArea);
            CellLayout previousCellLayout = previousLayout.Cells.TryFindPreviousCellLayout(cell.Id);

            (CellLayout cellLayout , ProcessingInfo processingInfo) = cell.CreateLayout(
                cellAvailableArea.Size,
                fieldVariables,
                previousCellLayout,
                services
            );

            processingInfos = [.. processingInfos, processingInfo];
            cellLayouts = [.. cellLayouts, cellLayout.Offset(cellAvailableArea.TopLeft)];
            cellLayouts = cellLayouts.AlignLayoutPartitions();

            gridLayout = gridLayout.JustifyGridRows(table.Id, cellLayout.BoundingBox.Size, cell.GridPosition, table.Grid);
            cellLayouts = cellLayouts.AlignCellHeights(gridLayout);

            columnsAvailableArea = gridLayout
                .SplitToColumnAreas(availableArea)
                .CropColumnsAvailableArea(cellLayouts);

            bool allColumnsFullyOccupied = cellLayouts.AllColumnsOccupied(gridLayout.Columns.Length);
            if (allColumnsFullyOccupied)
            {
                break;
            }
        }

        TableLayout tableLayout = cellLayouts
            .ComposeTableLayout(table, gridLayout)
            .Align(availableArea.Width, table.Alignment);

        ProcessingInfo tableProcessingInfo = processingInfos.CalculateProcessingInfo();
        return (tableLayout, tableProcessingInfo);
    }

    public static (TableLayout, UpdateInfo) Update(
        this TableLayout tableLayout,
        Table table,
        TableLayout previousTableLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        ILayoutServices services)
    {
        CellLayout[] cellLayouts = [];
        GridLayout gridLayout = table.InitializeGridLayout();
        Rectangle[] columnsAvailableArea = gridLayout.SplitToColumnAreas(availableArea);
        UpdateInfo[] updateInfos = [];

        foreach (CellLayout cellLayout in tableLayout.Cells)
        {
            Cell cell = table.Cells.Single(c => c.Id == cellLayout.ModelId);
            Rectangle cellAvailableArea = cell.CalculateCellAvailableArea(columnsAvailableArea);
            CellLayout previousPageCellLayout = previousTableLayout.Cells.TryFindPreviousCellLayout(cell.Id);
            (CellLayout updatedCellLayout, _) = cellLayout.Update(
                cell,
                cellAvailableArea.Size,
                fieldVariables,
                previousPageCellLayout,
                services
            );

            cellLayouts = [.. cellLayouts, updatedCellLayout.Offset(cellAvailableArea.TopLeft)];
            cellLayouts = cellLayouts.AlignLayoutPartitions();

            gridLayout = gridLayout.JustifyGridRows(table.Id, cellLayout.BoundingBox.Size, cell.GridPosition, table.Grid);
            cellLayouts = cellLayouts.AlignCellHeights(gridLayout);

            columnsAvailableArea = gridLayout
                .SplitToColumnAreas(availableArea)
                .CropColumnsAvailableArea(cellLayouts);

            bool allColumnsFullyOccupied = cellLayouts.AllColumnsOccupied(gridLayout.Columns.Length);
            if (allColumnsFullyOccupied)
            {
                break;
            }
        }

        TableLayout updatedTableLayout = cellLayouts
            .ComposeTableLayout(table, gridLayout)
            .Align(availableArea.Width, table.Alignment);

        UpdateInfo tableUpdateInfo = updateInfos.Any(ui => ui == UpdateInfo.ReconstructRequired)
            ? UpdateInfo.ReconstructRequired
            : UpdateInfo.Done;

        return (updatedTableLayout, tableUpdateInfo);
    }

    private static IEnumerable<Cell> InLayoutingOrder(this IEnumerable<Cell> cells) =>
        cells
            .OrderBy(c => c.GridPosition.Row)
            .ThenBy(c => c.GridPosition.Column);

    private static IEnumerable<Cell> SkipFinished(this IEnumerable<Cell> cells, CellLayout[] lastPageCellLayouts)
    {
        if (lastPageCellLayouts.Length == 0)
        {
            return cells;
        }

        return cells
            .SkipWhile(c => !lastPageCellLayouts.Any(l => l.ModelId == c.Id)) // skip cells processed on pages before last page
            .Where(c => !c.IsCellLayoutingFinished(lastPageCellLayouts))
            ;
    }

    private static TableLayout ComposeTableLayout(this CellLayout[] cellLayouts, Table table, GridLayout gridLayout)
    {
        Rectangle boundingBox = cellLayouts
            .CalculateBoundingBox(Rectangle.Empty);

        LayoutPartition layoutPartition = table.Cells.CalculateLayoutPartition(cellLayouts);

        TableLayout tableLayout = new(
            table.Id,
            cellLayouts,
            gridLayout,
            boundingBox,
            Borders.None,
            layoutPartition
        );

        return tableLayout;
    }

    private static TableLayout Align(this TableLayout layout, float availableWidth, Alignment alignment)
    {
        float freeSpace = availableWidth - layout.BoundingBox.Width;
        if (freeSpace <= 0) return layout;

        float xOffset = alignment switch
        {
            Alignment.Center => freeSpace / 2,
            Alignment.Right => freeSpace,
            _  => 0
        };

        return layout.Offset(new Position(xOffset, 0));
    }
}

file static class GridOperators
{
    public static Rectangle CalculateCellAvailableArea(this Cell cell, Rectangle[] columnsAvailableArea)
    {
        float x = columnsAvailableArea[cell.GridPosition.Column].X;
        float y = columnsAvailableArea
            .Skip(cell.GridPosition.Column)
            .Take(cell.GridPosition.ColumnSpan)
            .Max(r => r.Y);

        float totalWidth = columnsAvailableArea
            .Skip(cell.GridPosition.Column)
            .Take(cell.GridPosition.ColumnSpan)
            .Sum(r => r.Width);

        float bottom = columnsAvailableArea
            .Skip(cell.GridPosition.Column)
            .Take(cell.GridPosition.ColumnSpan)
            .Min(r => r.Bottom);

        return new Rectangle(x, y, totalWidth, bottom - y);
    }

    public static Rectangle[] CropColumnsAvailableArea(this Rectangle[] availableColumnsAreas, CellLayout[] cellLayouts) =>
        [
            ..availableColumnsAreas
                .Select((aa, index) =>
                {
                    float bottom = cellLayouts.CalculateBottom(index, aa);
                    return Rectangle.FromCorners(new Position(aa.X, bottom), aa.BottomRight);
                })
        ];

    private static float CalculateBottom(this CellLayout[] cellLayouts, int index, Rectangle cellColumn) =>
        cellLayouts
            .Where(l => l.GridPosition.ContainsColumnIndex(index))
            .Select(l => l.Partition.IsFinished()
                ? l.BoundingBox.Bottom
                : cellColumn.Bottom
            )
            .DefaultIfEmpty(cellColumn.Y)
            .Max();

    public static bool AllColumnsOccupied(this CellLayout[] cellLayouts, int colCount) =>
        Enumerable.Range(0, colCount)
            .Select(index => cellLayouts.IsColumnFullyOccupied(index))
            .All(isFullyOccupied => isFullyOccupied);

    private static bool IsColumnFullyOccupied(this CellLayout[] cellLayouts, int colIndex) =>
        cellLayouts
            .Where(l => l.GridPosition.ContainsColumnIndex(colIndex))
            .Select(l => l.Partition)
            .DefaultIfEmpty(LayoutPartition.StartEnd)
            .Any(lp => !lp.IsFinished());
}

file static class CellOperators
{
    public static CellLayout TryFindPreviousCellLayout(this CellLayout[] cellLayouts, ModelId cellId) =>
        cellLayouts.SingleOrDefault(c => c.ModelId == cellId, CellLayout.Empty);

    public static bool IsCellLayoutingFinished(this Cell cell, CellLayout[] lastPageCellLayouts) =>
        cell.GetLayoutPartitionOfCell(lastPageCellLayouts).IsFinished();

    private static LayoutPartition GetLayoutPartitionOfCell(this Cell cell, CellLayout[] lastPageCellLayouts) =>
        lastPageCellLayouts.SingleOrDefault(l => l.ModelId == cell.Id)?.Partition ?? LayoutPartition.Middle;
}

file static class ProcessingInfoOperators
{
    public static ProcessingInfo CalculateProcessingInfo(this ProcessingInfo[] cellProcessingInfos) =>
        cellProcessingInfos.Any(ip => ip is ProcessingInfo.RequestDrawingArea or ProcessingInfo.IgnoreAndRequestDrawingArea)
            ? ProcessingInfo.RequestDrawingArea
            : ProcessingInfo.Done;
}