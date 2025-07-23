using System;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class CellLayoutBuilder
{
    public static CellLayoutingResult Process(
        this Cell cell,
        CellLayoutingResult previousLayoutingResult,
        GridLayout grid,
        Rectangle availableArea,
        LayoutServices services)
    {
        bool isFinished = previousLayoutingResult.LastModelLayoutingResult.Status == ResultStatus.Finished;
        Model[] unprocessed = [
            ..cell
                .ParagraphsOrTables
                .SkipProcessed(previousLayoutingResult.LastModelLayoutingResult.ModelId, isFinished)
        ];

        LayoutingResult lastModelResult = NoLayoutingResult.Instance;
        Rectangle remainingArea = availableArea.Clip(cell.Padding);
        Layout[] layouts = [];
        ResultStatus resultStatus = ResultStatus.Finished;

        foreach (Model model in unprocessed)
        {
            LayoutingResult lr = model switch
            {
                Paragraph paragraph => paragraph.CreateParagraphLayout(previousLayoutingResult.LastModelLayoutingResult, remainingArea, services),
                // Table table => table.Cr(previousLayoutingResult.LastModelLayoutingResult, remainingArea, services),
                _ => NoLayoutingResult.Instance
            };

            if (lr.Status is ResultStatus.Finished or ResultStatus.RequestDrawingArea)
            {
                remainingArea = lr.RemainingDrawingArea;
                layouts = [.. layouts, .. lr.Layouts];
                lastModelResult = lr;
            }

            if (lr.Status != ResultStatus.Finished)
            {
                // remainingArea = new Rectangle(availableArea.BottomLeft, new Size(availableArea.Width, 0));
                resultStatus = ResultStatus.RequestDrawingArea;
                break;
            }
        }

        float minWidth = cell.MinWidth(grid);
        float minHeight = Math.Min(
            cell.MinHeight(grid) - previousLayoutingResult.CellLayout.BoundingBox.Height, // decrease height by already reserved height
            availableArea.Height // cell height cannot exceed available area
        ); 

        Rectangle minSize = new Rectangle(availableArea.X, availableArea.Y, minWidth, minHeight)
            .Clip(cell.Padding);

        Rectangle boundingBox = layouts
            .Select(l => l.BoundingBox)
            .Append(minSize) // append mininum bounding box if there are no elements or the elements have smaller width than the cell default
            .CalculateBoundingBox()
            .Expand(cell.Padding)
            ;

        CellLayout cellLayout = new(
            layouts,
            boundingBox,
            cell.Borders
        );

        return new CellLayoutingResult(
            cell.Id,
            previousLayoutingResult.Order + 1,
            cellLayout,
            cell.GridPosition,
            lastModelResult,
            remainingArea,
            resultStatus
        );
    }

    public static CellLayoutingResult[] UpdateByGrid(this CellLayoutingResult[] results, GridLayout grid) =>
        [
            ..results
                .Select(r =>r.UpdateByGrid(r.GridPosition, grid))
        ];

    private static CellLayoutingResult UpdateByGrid(
        this CellLayoutingResult result,
        GridPosition gridPosition,
        GridLayout grid)
    {
        float height = grid.CalculateCellAvailableHeight(gridPosition);
        if (result.CellLayout.BoundingBox.Height >= height)
        {
            return result;
        }

        return result with
        {
            CellLayout = result.CellLayout with
            {
                BoundingBox = result.CellLayout.BoundingBox.SetHeight(height)
            }
        };
    }

        private static ParagraphLayoutingResult CreateParagraphLayout(
        this Paragraph paragraph,
        LayoutingResult previousLayoutingResult,
        Rectangle remainingArea,
        LayoutServices services)
    {
        ParagraphLayoutingResult plr = previousLayoutingResult.AsResultOfModel(paragraph.Id, ParagraphLayoutingResult.None);
        return paragraph.Process(plr, remainingArea, services);
    }

    private static float MinWidth(this Cell cell, GridLayout grid) =>
        grid.CalculateCellWidth(cell.GridPosition);

    private static float MinHeight(this Cell cell, GridLayout grid) =>
        grid.CalculateCellAvailableHeight(cell.GridPosition); 
}
