using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
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

            contentLayouts = [..contentLayouts, ..contentLayoutingResult.Layouts];
            status = contentLayoutingResult.Status;
        }

        Rectangle boundingBox = contentLayouts
            .Select(l => l.BoundingBox)
            .CalculateBoundingBox()
            .Expand(cell.Padding);

        boundingBox = boundingBox
            .SetWidth(Math.Max(boundingBox.Width, availableArea.Width))
            .SetHeight(Math.Max(boundingBox.Height, 10))
            .MoveTo(availableArea.TopLeft)
            ;

        CellLayout cellLayout = new(
            new ModelReference([cell.Id]),
            contentLayouts,
            boundingBox,
            cell.Borders
        );

        return new CellLayoutingResult(
            cell.Id,
            previousLayoutingResult.Order + 1,
            cellLayout,
            ModelId.None,
            contentLayoutingResult,
            availableArea,
            ResultStatus.Finished
        );
    }


    private static ParagraphLayoutingResult CreateParagraphLayout(this Paragraph paragraph, Rectangle cellDrawingArea, LayoutServices services)
    {
        ParagraphLayoutingResult p = ParagraphLayoutingResult.New(cellDrawingArea);
        return paragraph.Process(p, cellDrawingArea, services);
    }
}
