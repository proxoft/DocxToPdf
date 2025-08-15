using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class TableLayoutBuilder
{
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
        Rectangle[] columnsAvailableArea = gridLayout.GridAvailableAreas(availableArea);
        bool[] columnFinished = [.. columnsAvailableArea.Select(_ => false)];

        foreach (Cell cell in table.Cells.InLayoutingOrder().SkipFinished(previosLayoutingResult.CellsLayoutingResults))
        {
            CellLayoutingResult[] previous = [
                ..previosLayoutingResult.CellsLayoutingResults
                .Where(r => r.ModelId == cell.Id)
            ];

            Rectangle cellAvailableArea = cell.CalculateCellAvailableArea(columnsAvailableArea);
            CellLayoutingResult result = cell.Process(previous, gridLayout, cellAvailableArea, fieldVariables, services);

            cellResults = [..cellResults, result];
            cellResults = cellResults.UpdateStatusByLastResult();

            gridLayout = gridLayout.JustifyGridRows(table.Id, result.CellLayout.BoundingBox.Size, cell.GridPosition, table.Grid);

            cellResults = cellResults.UpdateByGrid(gridLayout);
            columnsAvailableArea = gridLayout
                .GridAvailableAreas(availableArea)
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
            [..previosLayoutingResult.CellsLayoutingResults, ..cellResults], // collect all results
            new Rectangle(parentOffset.ShiftY(cropFromHeight), new Size(availableArea.Width, remainingHeight)),
            status
        );
    }

    private static IEnumerable<Cell> InLayoutingOrder(this IEnumerable<Cell> cells) =>
        cells
            .OrderBy(c => c.GridPosition.Row)
            .ThenBy(c => c.GridPosition.Column);

    private static IEnumerable<Cell> SkipFinished(this IEnumerable<Cell> cells, CellLayoutingResult[] results) =>
        cells
            .Where(c => results.All(r => !(r.ModelId == c.Id && r.Status == ResultStatus.Finished)));

    private static Rectangle CalculateCellAvailableArea(this Cell cell, Rectangle[] columnsAvailableArea)
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
