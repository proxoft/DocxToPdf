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
        ModelId lastProcessed = previousLayout.ModelId != paragraph.Id
            ? ModelId.None
            : previousLayout.LastProcessedElement();

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
        LayoutPartition lp = allDone.CalculateLayoutPartitionAfterUpdate(previousLayout.Partition);

        ParagraphLayout pl = new(
            paragraph.Id,
            lines,
            bb,
            Borders.None,
            lp
        );

        return (pl, processingInfo);
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
        (LineLayout[] lines, ProcessingInfo processingInfo) = unprocessed.CreateLineLayouts(0, availableSize, fieldVariables, paragraph.Style, services);

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
}

file static class ParagraphOperators
{
    public static ModelId LastProcessedElement(this ParagraphLayout paragraphLayout) =>
        paragraphLayout.Lines.LastProcessedElement();
}