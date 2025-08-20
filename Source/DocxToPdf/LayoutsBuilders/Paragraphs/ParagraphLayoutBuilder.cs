using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using DocumentFormat.OpenXml.Bibliography;
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

        (ParagraphLayout, ProcessingInfo) result = paragraph.CreateLayout(lastProcessed, availableSize, previousLayout.Partition, fieldVariables, services);
        return result;
    }

    public static (ParagraphLayout, ProcessingInfo) Update(
        this ParagraphLayout layout,
        Paragraph paragraph,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        (LineLayout[] lines, float spaceAfterLastLine, ProcessingInfo processingInfo) = layout.Lines.UpdateLineLayouts(
            paragraph,
            availableArea,
            fieldVariables,
            paragraph.Style,
            services
        );

        Rectangle bb = lines
            .Select(l => l.BoundingBox)
            .Append(new Rectangle(Position.Zero, new Size(availableArea.Width, 0))) // ensure full width size of paragraph
            .CalculateBoundingBox()
            .ExpandHeight(spaceAfterLastLine);

        // fix cases when ReconstructionRequired is returned. or unlike originally now DrawingAreaRequired (removes the End value)
        LayoutPartition lp = processingInfo.CalculateParagraphLayoutPartition(layout.Partition);

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
        (LineLayout[] lines, float spaceAfter, ProcessingInfo processingInfo) = unprocessed.CreateLineLayouts(availableSize, fieldVariables, paragraph.Style, services);

        Rectangle bb = lines
            .Select(l => l.BoundingBox)
            .Append(new Rectangle(Position.Zero, new Size(availableSize.Width, 0))) // ensure full width size of paragraph
            .CalculateBoundingBox()
            .ExpandHeight(spaceAfter);

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

    //private static ParagraphLayoutingResult UpdateInternal(
    //    this ParagraphLayout paragraphLayout,
    //    Paragraph paragraph,
    //    Position parentOffset,
    //    Size availableArea,
    //    FieldVariables fieldVariables,
    //    LayoutServices services)
    //{
    //    LineLayout[] updatedLines = [];
    //    float currentHeight = 0;

    //    foreach (LineLayout line in paragraphLayout.Lines)
    //    {
    //        (LineLayout ul, ProcessingInfo processingInfo) = line.Update(paragraph.Elements, new Rectangle(Position.Zero, availableArea), fieldVariables, services);
    //        if(ul.BoundingBox.Height + currentHeight > availableArea.Height)
    //        {
    //            break;
    //        }

    //        updatedLines = [.. updatedLines, ul];
    //        currentHeight += ul.BoundingBox.Height;
    //    }

    //    ResultStatus status = updatedLines.Length == paragraphLayout.Lines.Length
    //        ? ResultStatus.Finished
    //        : ResultStatus.ReconstructRequired;

    //    float spaceAfterLastLine = paragraphLayout.Partition is LayoutPartition.StartEnd or LayoutPartition.End
    //        && status == ResultStatus.Finished
    //        ? paragraph.Style.ParagraphSpacing.After
    //        : 0;

    //    Rectangle paragraphBb = updatedLines
    //        .Select(l => l.BoundingBox)
    //        .DefaultIfEmpty(Rectangle.Empty)
    //        .CalculateBoundingBox()
    //        .ExpandHeight(spaceAfterLastLine)
    //        .MoveTo(parentOffset)
    //        ;

    //    return new ParagraphLayoutingResult(
    //        paragraph.Id,
    //        new ParagraphLayout(
    //            paragraph.Id,
    //            updatedLines,
    //            paragraphBb,
    //            Borders.None,
    //            paragraphLayout.Partition
    //        ),
    //        ModelId.None, // TODO: fix
    //        new Rectangle(parentOffset, availableArea).CropFromTop(paragraphBb.Height),
    //        status
    //    );
    //}
}

file static class ParagraphOperators
{
    public static ModelId LastProcessedElement(this ParagraphLayout paragraphLayout) =>
        paragraphLayout.Lines.Length == 0
            ? ModelId.None
            : paragraphLayout.Lines.Last().LastProcessedElement();


    public static ModelId LastProcessedElement(this LineLayout lineLayout) =>
        lineLayout.Words.Length == 0
            ? ModelId.None
            : lineLayout.Words.Last().ModelId;
}