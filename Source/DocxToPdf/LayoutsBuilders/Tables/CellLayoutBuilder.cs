using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Extensions;
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
        Rectangle remainingArea = availableArea.Clip(cell.Padding);
        Layout[] contentLayouts = [];

        Stack<Model> toProcess = cell.ParagraphsOrTables
           .SkipProcessed(previousLayoutingResult.LastProcessedModel, finished: previousLayoutingResult.LastModelLayoutingResult.Status == ResultStatus.Finished)
           .ToStackReversed();

        ResultStatus status = ResultStatus.Finished;
        LayoutingResult contentLayoutingResult = NoLayoutingResult.Instance;

        while (toProcess.Count > 0 && status == ResultStatus.Finished)
        {
            Model model = toProcess.Pop();
            contentLayoutingResult = model switch
            {
                Paragraph p => p.CreateParagraphLayout(remainingArea, services),
                _ => NoLayoutingResult.Instance
            };

            if(contentLayoutingResult.Status != ResultStatus.Ignore)
            {
                contentLayouts = [.. contentLayouts, .. contentLayoutingResult.Layouts];
            }

            status = contentLayoutingResult.Status == ResultStatus.Ignore
                ? ResultStatus.RequestDrawingArea
                : contentLayoutingResult.Status;

            remainingArea = contentLayoutingResult.RemainingDrawingArea;
        }

        float minWidth = cell.MinWidth(grid);
        float minHeight = cell.MinHeight(grid);
        Rectangle minSize = new Rectangle(availableArea.X, availableArea.Y, minWidth, minHeight)
            .Clip(cell.Padding);

        Rectangle boundingBox = contentLayouts
            .Select(l => l.BoundingBox)
            .Append(minSize)
            .CalculateBoundingBox()
            .Expand(cell.Padding)
            ;

        boundingBox = boundingBox
            .SetWidth(Math.Max(boundingBox.Width, minWidth))
            .SetHeight(Math.Max(boundingBox.Height, minHeight))
            .MoveTo(availableArea.TopLeft)
            ;

        CellLayout cellLayout = new(
            contentLayouts,
            boundingBox,
            cell.Borders
        );

        return new CellLayoutingResult(
            cell.Id,
            previousLayoutingResult.Order + 1,
            cellLayout,
            cell.GridPosition,
            ModelId.None,
            contentLayoutingResult,
            availableArea,
            status
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

    private static ParagraphLayoutingResult CreateParagraphLayout(this Paragraph paragraph, Rectangle cellDrawingArea, LayoutServices services)
    {
        ParagraphLayoutingResult p = ParagraphLayoutingResult.New(cellDrawingArea);
        return paragraph.Process(p, cellDrawingArea, services);
    }

    private static float MinWidth(this Cell cell, GridLayout grid) =>
        grid.CalculateCellWidth(cell.GridPosition);

    private static float MinHeight(this Cell cell, GridLayout grid) =>
        grid.CalculateCellAvailableHeight(cell.GridPosition); 
}
