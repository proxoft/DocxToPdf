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
        ModelId lastProcessed = previousLayout.LastProcessedElementIdOf(paragraph.Id);

        FixedImageLayout[] fixedImageLayouts = paragraph.FixedDrawings.CreateFixedImageLayouts(availableArea, previousLayout.FixedImages);
        Element[] unprocessed = [.. paragraph.Elements.SkipProcessed(lastProcessed, true)];
        ParagraphLayoutingArea area = paragraph.CreateArea(availableArea);

        (LineLayout[] lines, ProcessingInfo processingInfo) = unprocessed.CreateLineLayouts(area, fieldVariables, paragraph.Style, services);
        if (lines.Length == 0)
        {
            return (ParagraphLayout.Empty, processingInfo);
        }

        Rectangle bb = lines
            .CalculateBoundingBox(Rectangle.Empty)
            .SetWidth(availableArea.Width) // ensure full width size of paragraph
            ;

        LayoutPartition layoutPartition = paragraph.CalculateLayoutPartition(lines);
        ParagraphLayout paragraphLayout = new(
            paragraph.Id,
            lines,
            fixedImageLayouts,
            bb,
            Borders.None,
            layoutPartition
        );

        return (paragraphLayout, processingInfo);
    }

    public static (ParagraphLayout, UpdateInfo) Update(
        this ParagraphLayout layout,
        Paragraph paragraph,
        ParagraphLayout previousLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        FixedImageLayout[] updatedFixedImages = layout.FixedImages.Update(paragraph.FixedDrawings, availableArea);
        ParagraphLayoutingArea area = paragraph.CreateArea(availableArea);
        (LineLayout[] lines, UpdateInfo updateInfo) = layout.Lines.UpdateLineLayouts(
            paragraph,
            area,
            fieldVariables,
            services
        );

        Rectangle bb = lines
            .CalculateBoundingBox(Rectangle.Empty)
            .SetWidth(availableArea.Width);

        LayoutPartition layoutPartition = paragraph.CalculateLayoutPartition(lines);
        ParagraphLayout pl = new(
            paragraph.Id,
            lines,
            updatedFixedImages,
            bb,
            Borders.None,
            layoutPartition
        );

        return (pl, updateInfo);
    }
}

file static class ParagraphOperators
{
    public static ModelId LastProcessedElementIdOf(this ParagraphLayout paragraphLayout, ModelId ofParagraph) =>
        paragraphLayout.ModelId == ofParagraph
            ? paragraphLayout.Lines.LastProcessedElementId()
            : ModelId.None;

    public static ParagraphLayoutingArea CreateArea(this Paragraph paragraph, Size availableArea)
    {
        Rectangle[] reservedSpaces = paragraph.FixedDrawings.CreateReservedSpaces();
        return new ParagraphLayoutingArea(availableArea, 0, 0, reservedSpaces);
    }

    public static LayoutPartition CalculateLayoutPartition(this Paragraph paragraph, LineLayout[] lines) =>
        paragraph.Elements.CalculateLayoutPartition([..lines.SelectMany(l => l.Words)]);
}