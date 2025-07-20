using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
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
        // split area
        foreach (Cell cell in table.Cells.InLayoutingOrder())
        {
            Rectangle cellAvailableArea = table.Grid.CalculateCellAvailableArea(cell, availableArea);
            LayoutingResult result = cell.Process(availableArea, services);
        }

        return new TableLayoutingResult(
            [],
            ModelId.None,
            [],
            availableArea,
            ResultStatus.Finished
        );
    }

    private static IEnumerable<Cell> InLayoutingOrder(this IEnumerable<Cell> cells) =>
        cells
            .OrderBy(c => c.GridPosition.Row)
            .ThenBy(c => c.GridPosition.Column);

    private static Rectangle CalculateCellAvailableArea(this Grid grid, Cell cell, Rectangle tableAvailableArea)
    {
        (float offset, float width) = grid.CalculateCellRegion(cell.GridPosition);

        Rectangle cellArea = tableAvailableArea
            .CropFromLeft(offset)
            .CropWidth(width);

        return cellArea;
    }
}
