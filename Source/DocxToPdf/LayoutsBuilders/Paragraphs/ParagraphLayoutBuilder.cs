using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.ExtendedProperties;
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
        return paragraphLayout.UpdateInternal(paragraph, availableArea.TopLeft, availableArea.Size, fieldVariables, services);
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

    private static ParagraphLayoutingResult UpdateInternal(
        this ParagraphLayout paragraphLayout,
        Paragraph paragraph,
        Position parentOffset,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        LineLayout[] updatedLines = [];
        float currentHeight = 0;

        foreach (LineLayout line in paragraphLayout.Lines)
        {
            (LineLayout ul, bool containsAll) = line.Update(paragraph.Elements, new Rectangle(Position.Zero, availableArea), fieldVariables, services);
            if(ul.BoundingBox.Height + currentHeight > availableArea.Height)
            {
                break;
            }

            updatedLines = [.. updatedLines, ul];
            currentHeight += ul.BoundingBox.Height;
        }

        ResultStatus status = updatedLines.Length == paragraphLayout.Lines.Length
            ? ResultStatus.Finished
            : ResultStatus.ReconstructRequired;

        float spaceAfterLastLine = paragraphLayout.Partition is LayoutPartition.StartEnd or LayoutPartition.End
            && status == ResultStatus.Finished
            ? paragraph.Style.ParagraphSpacing.After
            : 0;

        Rectangle paragraphBb = updatedLines
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(Rectangle.Empty)
            .CalculateBoundingBox()
            .ExpandHeight(spaceAfterLastLine)
            .MoveTo(parentOffset)
            ;

        return new ParagraphLayoutingResult(
            paragraph.Id,
            new ParagraphLayout(
                paragraph.Id,
                updatedLines,
                paragraphBb,
                Borders.None,
                paragraphLayout.Partition
            ),
            ModelId.None, // TODO: fix
            new Rectangle(parentOffset, availableArea).CropFromTop(paragraphBb.Height),
            status
        );
    }
}