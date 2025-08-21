using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class ParagraphLayoutBuilder
{
    public static (ParagraphLayout , ProcessingInfo) CreateLayout(
        this Paragraph paragraph,
        ParagraphLayout previousLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size availableSize = previousLayout.ModelId == paragraph.Id
            ? availableArea
            : availableArea.DecreaseHeight(paragraph.Style.ParagraphSpacing.Before);

        ModelId lastProcessed = previousLayout.ModelId != paragraph.Id
            ? ModelId.None
            : previousLayout.LastProcessedElement();

        float spaceBefore = paragraph.Style.ParagraphSpacing.Before;
        Size remainingArea = availableArea;

        (ParagraphLayout, ProcessingInfo) result = paragraph.CreateLayout(lastProcessed, remainingArea, previousLayout.Partition, fieldVariables, services);
        return result;
    }

    public static (ParagraphLayout, ProcessingInfo) Update(
        this ParagraphLayout layout,
        Paragraph paragraph,
        ParagraphLayout previousLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        (LineLayout[] lines, ProcessingInfo processingInfo) = layout.Lines.UpdateLineLayouts(
            paragraph,
            availableArea,
            fieldVariables,
            paragraph.Style,
            services
        );

        Rectangle bb = lines
            .CalculateBoundingBox(Rectangle.Empty)
            .SetWidth(availableArea.Width);

        bool allDone = paragraph.Elements.Select(e => e.Id).LastOrDefault(ModelId.None) == lines.LastProcessedElement();
        LayoutPartition lp = processingInfo.CalculateLayoutPartitionAfterUpdate(previousLayout.Partition, allDone);

        ParagraphLayout pl = new(
            paragraph.Id,
            lines,
            bb,
            Borders.None,
            lp
        );

        return (pl, processingInfo);
    }

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

    private static (ParagraphLayout, ProcessingInfo) CreateLayout(
        this Paragraph paragraph,
        ModelId lastProcessed,
        Size availableSize,
        LayoutPartition previousPartition,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Element[] unprocessed = [..paragraph.Elements.SkipProcessed(lastProcessed, true)];
        (LineLayout[] lines, ProcessingInfo processingInfo) = unprocessed.CreateLineLayouts(availableSize, fieldVariables, paragraph.Style, services);

        Rectangle bb = lines
            .CalculateBoundingBox(Rectangle.Empty)
            .SetWidth(availableSize.Width) // ensure full width size of paragraph
            ;

        LayoutPartition layoutPartition = processingInfo.CalculateParagraphLayoutPartition(previousPartition);
        ParagraphLayout paragraphLayout = new(
            paragraph.Id,
            lines,
            bb,
            Borders.None,
            layoutPartition
        );

        return (paragraphLayout, processingInfo);
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

file static class ParagraphOperators
{
    public static ModelId LastProcessedElement(this ParagraphLayout paragraphLayout) =>
        paragraphLayout.Lines.LastProcessedElement();
}