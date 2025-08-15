using System;
using System.Collections.Generic;
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
        CellLayoutingResult[] previousLayoutingResults,
        GridLayout grid,
        Rectangle availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        LayoutingResult lastChildResult= previousLayoutingResults.LastByOrder().LastModelLayoutingResult;
        Model[] unprocessed = cell.ParagraphsOrTables.SkipProcessed(lastChildResult);

        Rectangle remainingArea = availableArea.MoveTo(Position.Zero).Clip(cell.Padding);
        LayoutingResult lastModelResult = previousLayoutingResults.Length == 0
            ? NoLayoutingResult.Create(remainingArea)
            : lastChildResult;

        Layout[] layouts = [];
        ResultStatus resultStatus = ResultStatus.Finished;

        foreach (Model model in unprocessed)
        {
            LayoutingResult lr = model switch
            {
                Paragraph paragraph => paragraph.Process(lastModelResult, remainingArea, fieldVariables, services),
                Table table => table.Process(lastModelResult, remainingArea, fieldVariables, services),
                _ => NoLayoutingResult.Create(remainingArea)
            };

            if (lr.Status is ResultStatus.Finished
                or ResultStatus.RequestDrawingArea
                or ResultStatus.NewPageRequired)
            {
                remainingArea = lr.RemainingDrawingArea;
                layouts = [.. layouts, .. lr.Layouts];
                lastModelResult = lr;
            }

            if (lr.Status != ResultStatus.Finished)
            {
                resultStatus = ResultStatus.RequestDrawingArea;
                break;
            }
        }

        float minWidth = cell.MinWidth(grid);
        float minHeight = Math.Min(
            cell.MinHeight(grid), // decrease height by already reserved height
            0 // cell height cannot exceed available area
        );

        Rectangle minSize = new Rectangle(Position.Zero, new Size(minWidth, minHeight))
            .Clip(cell.Padding);

        Rectangle boundingBox = layouts
            .Select(l => l.BoundingBox)
            .Append(minSize) // append mininum bounding box if there are no elements or the elements have smaller width than the cell default
            .CalculateBoundingBox()
            .Expand(cell.Padding)
            .MoveTo(availableArea.TopLeft)
            ;

        LayoutPartition partition = resultStatus.CalculateLayoutPartition(previousLayoutingResults.LastByOrder());

        CellLayout cellLayout = new(
            cell.Id,
            layouts,
            boundingBox,
            cell.Borders,
            partition
        );

        Rectangle remArea = availableArea
            .CropFromTop(boundingBox.Height);

        return new CellLayoutingResult(
            cell.Id,
            previousLayoutingResults.LastByOrder().Order + 1,
            cellLayout,
            cell.GridPosition,
            lastModelResult,
            remArea,
            resultStatus
        );
    }

    public static CellLayoutingResult[] UpdateStatusByLastResult(
        this CellLayoutingResult[] results)
    {
        if(results.Length <= 1)
        {
            return results;
        }

        CellLayoutingResult last = results[^1];
        if(last.Status == ResultStatus.Finished)
        {
            // check previous
            bool containsNotFinished = results
                .Any(r => r.Status != ResultStatus.Finished && r.GridPosition.BottomRow() == last.GridPosition.BottomRow());

            if (containsNotFinished)
            {
                last = last.ForceRequestDrawingAreaStatus();
                return [.. results.SkipLast(1), last];
            }
            else
            {
                return results;
            }
        }
        else
        {
            CellLayoutingResult[] updated = [
                ..results
                    .Select(res =>
                        res == last || res.GridPosition.BottomRow() != last.GridPosition.BottomRow()
                        ? res
                        : res.ForceRequestDrawingAreaStatus())
            ];

            return updated;
        }
    }

    public static CellLayoutingResult[] UpdateByGrid(
        this CellLayoutingResult[] results,
        GridLayout grid) =>
        [
            ..results
                .Select(r => r.UpdateByGrid(r.GridPosition, grid))
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

    private static CellLayoutingResult ForceRequestDrawingAreaStatus(this CellLayoutingResult result)
    {
        CellLayout cellLayout = result.CellLayout with
        {
            Partition = result.CellLayout.Partition.RemoveEnd()
        };

        CellLayoutingResult r = result with
        {
            CellLayout = cellLayout,
            Status = ResultStatus.RequestDrawingArea
        };

        return r;
    }

    private static float MinWidth(this Cell cell, GridLayout grid) =>
        grid.CalculateCellWidth(cell.GridPosition);

    private static float MinHeight(this Cell cell, GridLayout grid) =>
        grid.CalculateCellAvailableHeight(cell.GridPosition);

    public static Model[] FilterProcessed(this IEnumerable<Model> models, CellLayoutingResult[] previousResults)
    {
        CellLayoutingResult lastResult = previousResults.LastByOrder();
        bool isFinished = lastResult.LastModelLayoutingResult.Status == ResultStatus.Finished;
        return [.. models.SkipProcessed(lastResult.LastModelLayoutingResult.ModelId, isFinished)];
    }
}
