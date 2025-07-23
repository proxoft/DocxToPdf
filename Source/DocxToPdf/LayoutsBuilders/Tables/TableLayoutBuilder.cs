using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class TableLayoutBuilder
{
    public static TableLayoutingResult Process(
        this Table table,
        TableLayoutingResult previosLayoutingResult,
        Rectangle availableArea,
        LayoutServices services)
    {
        GridLayout gridLayout = previosLayoutingResult.Grid == GridLayout.Empty
            ? table.Grid.CreateGridLayout()
            : previosLayoutingResult.Grid;

        CellLayoutingResult[] cellResults = [];
        Rectangle[] columnsAvailableArea = gridLayout.GridAvailableAreas(availableArea);
        bool[] columnFinished = [.. columnsAvailableArea.Select(_ => false)];

        foreach (Cell cell in table.Cells.InLayoutingOrder().SkipFinished(previosLayoutingResult.CellsLayoutingResult))
        {
            CellLayoutingResult previous = previosLayoutingResult.CellsLayoutingResult
                .OrderByDescending(r => r.Order)
                .FirstOrDefault(r => r.ModelId == cell.Id, CellLayoutingResult.None);

            Rectangle cellAvailableArea = cell.CalculateCellAvailableArea(columnsAvailableArea);
            CellLayoutingResult result = cell.Process(previous, gridLayout, cellAvailableArea, services);
            cellResults = [..cellResults, result];
            gridLayout = gridLayout.JustifyGridRows(result.CellLayout.BoundingBox, cell.GridPosition);

            cellResults = cellResults.UpdateByGrid(gridLayout);
            columnsAvailableArea = columnsAvailableArea.CropColumnsAvailableArea(cellResults);

            if(result.Status != ResultStatus.Finished)
            {
                for(int i = cell.GridPosition.Column; i < cell.GridPosition.Column + cell.GridPosition.ColumnSpan; i++)
                {
                    columnFinished[i] = true;
                }
            }

            if(columnFinished.All(c => c == true))
            {
                break;
            }
        }

        Rectangle boundingBox = cellResults
            .Select(r => r.CellLayout)
            .Select(c => c.BoundingBox)
            .CalculateBoundingBox();

        TableLayout tableLayout = new(
            [.. cellResults.Select(r => r.CellLayout)],
            boundingBox,
            Borders.None
        );

        ResultStatus status = cellResults.Any(r => r.Status == ResultStatus.RequestDrawingArea)
            ? ResultStatus.RequestDrawingArea
            : ResultStatus.Finished;

        return new TableLayoutingResult(
            table.Id,
            tableLayout,
            gridLayout,
            cellResults,
            availableArea,
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
                    .Select(r => r.CellLayout.BoundingBox.Bottom)
                    .DefaultIfEmpty(aa.Y)
                    .Max();

                return Rectangle.FromCorners(new Position(aa.X, bottom), aa.BottomRight);
            })
        ];
}
