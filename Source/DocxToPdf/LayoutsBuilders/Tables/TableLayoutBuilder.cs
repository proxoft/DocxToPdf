using System;
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
        LayoutServices services)
    {
        GridLayout gridLayout = table.Grid.InitializeGridLayout(table.Id);
        Rectangle[] columnsAvailableArea = gridLayout.SplitToColumnAreas(availableArea);
        bool[] columnFinished = [.. columnsAvailableArea.Select(_ => false)];
        CellLayout[] cellLayouts = [];
        ProcessingInfo[] processingInfos = [];

        foreach (Cell cell in table.Cells.InLayoutingOrder().SkipFinished(previousLayout.Cells))
        {
            Rectangle cellAvailableArea = cell.CalculateCellAvailableArea(columnsAvailableArea);
            CellLayout previousCellLayout = previousLayout.Cells.TryFindPreviousCellLayout(cell.Id);

            (CellLayout cellLayout , ProcessingInfo processingInfo) = cell.CreateLayout(cellAvailableArea.Size, fieldVariables, gridLayout, previousCellLayout, services);

            processingInfos = [.. processingInfos, processingInfo];
            cellLayouts = [.. cellLayouts, cellLayout.SetOffset(cellAvailableArea.TopLeft)];
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

        Rectangle boundingBox = cellLayouts
            .CalculateBoundingBox(Rectangle.Empty);

        LayoutPartition layoutPartition = cellLayouts
            .Select(c => c.Partition)
            .CalculateLayoutPartition(previousLayout.Partition);

        TableLayout tableLayout = new(
            table.Id,
            cellLayouts,
            boundingBox,
            Borders.None,
            layoutPartition
        );

        ProcessingInfo tableProcessingInfo = processingInfos.CalculateProcessingInfo();
        return (tableLayout, tableProcessingInfo);
    }

    public static (TableLayout, ProcessingInfo) Update(
        this TableLayout tableLayout)
    {
        return (tableLayout, ProcessingInfo.Done);
    }

    public static TableLayoutingResult Process(this Table table, LayoutingResult previousResult, Rectangle remainingArea, FieldVariables fieldVariables, LayoutServices services)
    {
        TableLayoutingResult tlr = previousResult.AsResultOfModel(table.Id, TableLayoutingResult.None);
        return table.ProcessInternal(tlr, remainingArea.TopLeft, remainingArea.Size, fieldVariables, services);
    }

    private static TableLayoutingResult ProcessInternal(
        this Table table,
        TableLayoutingResult previosLayoutingResult,
        Position parentOffset,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        GridLayout gridLayout = table.Grid.InitializeGridLayout(table.Id);

        CellLayoutingResult[] cellResults = [];
        Rectangle[] columnsAvailableArea = gridLayout.SplitToColumnAreas(availableArea);
        bool[] columnFinished = [.. columnsAvailableArea.Select(_ => false)];

        foreach (Cell cell in table.Cells.InLayoutingOrder().SkipFinished(previosLayoutingResult.CellsLayoutingResults))
        {
            CellLayoutingResult[] previous = [
                ..previosLayoutingResult.CellsLayoutingResults
                .Where(r => r.ModelId == cell.Id)
            ];

            Rectangle cellAvailableArea = cell.CalculateCellAvailableArea(columnsAvailableArea);
            CellLayoutingResult result = cell.Process(previous, gridLayout, cellAvailableArea, fieldVariables, services);

            cellResults = [.. cellResults, result];
            cellResults = cellResults.UpdateStatusByLastResult();

            gridLayout = gridLayout.JustifyGridRows(table.Id, result.CellLayout.BoundingBox.Size, cell.GridPosition, table.Grid);

            cellResults = cellResults.UpdateByGrid(gridLayout);
            columnsAvailableArea = gridLayout
                .SplitToColumnAreas(availableArea)
                .CropColumnsAvailableArea(cellResults);

            bool allColumnsFinished = cellResults.AllColumnsFinished(gridLayout.Columns.Length);
            if (allColumnsFinished)
            {
                break;
            }
        }

        Rectangle boundingBox = cellResults
            .Select(r => r.CellLayout)
            .Select(c => c.BoundingBox)
            .CalculateBoundingBox()
            .MoveTo(parentOffset);

        float cropFromHeight = boundingBox.Height;
        float remainingHeight = Math.Max(0, availableArea.Height - cropFromHeight);

        ResultStatus status = cellResults.Any(r => r.Status == ResultStatus.RequestDrawingArea)
            ? ResultStatus.RequestDrawingArea
            : ResultStatus.Finished;

        LayoutPartition partition = status.CalculateLayoutPartition(previosLayoutingResult);

        TableLayout tableLayout = new(
            table.Id,
            [.. cellResults.Select(r => r.CellLayout)],
            boundingBox,
            Borders.None,
            partition
        );

        return new TableLayoutingResult(
            table.Id,
            tableLayout,
            gridLayout,
            [.. previosLayoutingResult.CellsLayoutingResults, .. cellResults], // collect all results
            new Rectangle(parentOffset.ShiftY(cropFromHeight), new Size(availableArea.Width, remainingHeight)),
            status
        );
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

    private static IEnumerable<Cell> SkipFinished(this IEnumerable<Cell> cells, CellLayoutingResult[] results) =>
        cells
            .Where(c => results.All(r => !(r.ModelId == c.Id && r.Status == ResultStatus.Finished)));

    private static Rectangle[] CropColumnsAvailableArea(this Rectangle[] availableColumnsAreas, CellLayoutingResult[] cellResults) =>
        [..availableColumnsAreas
            .Select((aa, index) =>
            {
                float bottom = cellResults
                    .Where(r => r.GridPosition.ContainsColumnIndex(index))
                    .Select(r => r.Status == ResultStatus.RequestDrawingArea
                        ? aa.Bottom
                        : r.CellLayout.BoundingBox.Bottom
                    )
                    .DefaultIfEmpty(aa.Y)
                    .Max();

                return Rectangle.FromCorners(new Position(aa.X, bottom), aa.BottomRight);
            })
        ];

    private static bool AllColumnsFinished(this CellLayoutingResult[] results, int colCount) =>
        Enumerable.Range(0, colCount)
            .Select(index => results.IsColumnFinished(index))
            .All(isFinished => isFinished);

    private static bool IsColumnFinished(this CellLayoutingResult[] results, int colIndex) =>
        results
            .Where(r => r.GridPosition.ContainsColumnIndex(colIndex))
            .Select(r => r.Status)
            .DefaultIfEmpty(ResultStatus.Finished)
            .Any(s => s == ResultStatus.RequestDrawingArea);
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

file static class LayoutPartitionOperators
{
    public static LayoutPartition CalculateLayoutPartition(this IEnumerable<LayoutPartition> cellLayoutPartitions, LayoutPartition previousTablePartition)
    {
        LayoutPartition result = LayoutPartition.Middle;
        if(previousTablePartition.IsFinished())
        {
            result |= LayoutPartition.Start;
        }

        if(cellLayoutPartitions.All(lp => lp.IsFinished()))
        {
            result |= LayoutPartition.End;
        }

        return result;
    }
}