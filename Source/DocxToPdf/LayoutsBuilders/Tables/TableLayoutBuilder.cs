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
        CellLayoutingResult[] cellResults = [];
        CellLayout[] cellLayouts = [];
        Rectangle[] columnsAvailableArea = table.Grid.GridAvailableAreas(availableArea);

        // split area
        foreach (Cell cell in table.Cells.InLayoutingOrder())
        {
            CellLayoutingResult previous = previosLayoutingResult.CellsLayoutingResult
                .OrderByDescending(r => r.Order)
                .FirstOrDefault(r => r.CellId == cell.Id, CellLayoutingResult.None);

            Rectangle cellAvailableArea = cell.CalculateCellAvailableArea(columnsAvailableArea);
            CellLayoutingResult result = cell.Process(previous, table.Grid, cellAvailableArea, services);
            cellResults = [..cellResults, result];
            cellLayouts = [..cellLayouts, result.CellLayout];
            columnsAvailableArea = columnsAvailableArea.CropClumnsAvailableArea(result.CellLayout.BoundingBox, cell.GridPosition);
        }

        Rectangle boundingBox = cellLayouts
            .Select(c => c.BoundingBox)
            .CalculateBoundingBox();

        return new TableLayoutingResult(
            table.Id,
            new TableLayout(new Layouts.ModelReference([]), cellLayouts, boundingBox, Borders.None),
            GridLayout.Empty,
            cellResults,
            availableArea,
            ResultStatus.Finished
        );
    }

    private static IEnumerable<Cell> InLayoutingOrder(this IEnumerable<Cell> cells) =>
        cells
            .OrderBy(c => c.GridPosition.Row)
            .ThenBy(c => c.GridPosition.Column);

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

    private static Rectangle[] CropClumnsAvailableArea(this Rectangle[] columnsAvailableArea, Rectangle occupiedArea, GridPosition byCell) =>
        [
            ..columnsAvailableArea
                .Select((columnArea, index) =>
                {
                    if(index < byCell.Column || index >= byCell.Column + byCell.ColumnSpan)
                        return columnArea;

                    return columnArea.CropFromTop(occupiedArea.Height);
                })
        ];
}
