using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class ParagraphLayoutBuilder
{
    public static ParagraphLayoutingResult Process(
        this Paragraph paragraph,
        LayoutingResult previousLayoutingResult,
        Rectangle availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        ParagraphLayoutingResult plr = previousLayoutingResult.AsResultOfModel(paragraph.Id, ParagraphLayoutingResult.None);
        if(plr.Status == ResultStatus.NewPageRequired)
        {
            return new ParagraphLayoutingResult(paragraph.Id, ParagraphLayout.Empty, ModelId.None, availableArea, ResultStatus.Finished);
        }

        Rectangle ar = plr.LastProcessedModelId == ModelId.None
            ? availableArea
            : availableArea.CropFromTop(paragraph.Style.ParagraphSpacing.Before);

        return paragraph.ProcessInternal(plr, ar.TopLeft, ar.Size, fieldVariables, services);
    }

    public static ParagraphLayoutingResult Update(
        this ParagraphLayout paragraphLayout,
        Paragraph paragraph,
        Rectangle availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        LineLayout[] lines = [];

        return ParagraphLayoutingResult.None with
        {
            ModelId = paragraph.Id
        };
    }

    private static ParagraphLayoutingResult ProcessInternal(
        this Paragraph paragraph,
        ParagraphLayoutingResult previousLayoutingResult,
        Position parentOffset,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        IEnumerable<Element> unprocessed = paragraph.Elements
            .SkipProcessed(previousLayoutingResult.LastProcessedModelId, finished: true);

        (LineLayout[] lines, float spaceAfterLastLine, ModelId lastProcessedElementId, ResultStatus status) = unprocessed.CreateLines(new Rectangle(Position.Zero, availableArea), fieldVariables, paragraph.Style, services);

        Rectangle paragraphBb = lines
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(Rectangle.Empty)
            .CalculateBoundingBox()
            .ExpandHeight(spaceAfterLastLine)
            .MoveTo(parentOffset)
            ;

        float cropFromHeight = paragraphBb.Height;
        LayoutPartition partition = status.CalculateLayoutPartition(previousLayoutingResult);
        ParagraphLayout pl = new(paragraph.Id, [.. lines], paragraphBb, Borders.None, partition);

        if(status == ResultStatus.Finished)
        {
            cropFromHeight = Math.Min(cropFromHeight + paragraph.Style.ParagraphSpacing.After, availableArea.Height);
        }

        float remainingHeight = Math.Max(0, availableArea.Height - cropFromHeight);

        return new ParagraphLayoutingResult(
            paragraph.Id,
            pl,
            lastProcessedElementId,
            new Rectangle(parentOffset.ShiftY(cropFromHeight), new Size(availableArea.Width, remainingHeight)),
            status
        );
    }
}