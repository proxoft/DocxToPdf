using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Tables;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Sections;

internal static class SectionLayoutBuilder
{
    public static (SectionLayout, ProcessingInfo) CreateLayout(
        this Section section,
        SectionLayout previousSectionLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Model[] unprocessed = section.Unprocessed(previousSectionLayout.Layouts);
        Layout[] layouts = [];
        Size remainingArea = availableArea;
        ProcessingInfo sectionProcessingInfo = ProcessingInfo.Done;
        float yOffset = 0;

        foreach (Model model in unprocessed)
        {
            (Layout layout, ProcessingInfo processingInfo) result = model switch
            {
                Paragraph p => p.CreateLayout(
                    previousSectionLayout.TryFindParagraphLayout(p.Id),
                    remainingArea,
                    fieldVariables,
                    services),
                Table t => t.CreateTableLayout(
                    previousSectionLayout.TryFindTableLayout(t.Id),
                    remainingArea,
                    fieldVariables,
                    services),
                _ => (NoLayout.Instance, ProcessingInfo.Done)
            };

            sectionProcessingInfo = result.processingInfo;

            if(result.layout.IsNotEmpty())
            {
                remainingArea = remainingArea.DecreaseHeight(result.layout.BoundingBox.Height);
                layouts = [.. layouts, result.layout.SetOffset(new Position(0, yOffset))];
                yOffset += result.layout.BoundingBox.Height;
            }

            if (sectionProcessingInfo is ProcessingInfo.NewPageRequired
                or ProcessingInfo.RequestDrawingArea
                or ProcessingInfo.IgnoreAndRequestDrawingArea)
            {
                break;
            }

            Model nextModel = unprocessed.Next(model.Id);
            float spaceAfter = model.CalculateSpaceAfter(result.layout.Partition, nextModel);
            yOffset += spaceAfter;
            remainingArea = remainingArea.DecreaseHeight(spaceAfter);
        }

        Rectangle boudingBox = layouts
            .CalculateBoundingBox(Rectangle.Empty);

        LayoutPartition layoutPartition = sectionProcessingInfo.CalculateLayoutPartition(previousSectionLayout.Partition);

        SectionLayout sectionLayout = new(
            section.Id,
            layouts,
            boudingBox,
            Borders.None,
            layoutPartition
        );

        return (sectionLayout, sectionProcessingInfo);
    }

    public static (SectionLayout, UpdateInfo) Update(
        this SectionLayout sectionLayout,
        Section section,
        SectionLayout previousSectionLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = availableArea;
        Layout[] updatedLayouts = [];
        // ProcessingInfo sectionProcessingInfo = ProcessingInfo.Done;
        UpdateInfo lastUpdateInfo = UpdateInfo.Done;
        float yOffset = 0;

        foreach (Layout layout in sectionLayout.Layouts)
        {
            (Layout layout, UpdateInfo updateInfo) result = layout switch
            {
                ParagraphLayout pl => pl.Update(
                    section.Find<Paragraph>(pl.ModelId),
                    previousSectionLayout.Layouts.LastOfTypeOr(ParagraphLayout.Empty), // try find in previous section
                    remainingArea,
                    fieldVariables,
                    services
                ),
                TableLayout tl => tl.Update(
                    section.Find<Table>(tl.ModelId),
                    previousSectionLayout.Layouts.LastOfTypeOr(TableLayout.Empty), // try find in previous section
                    remainingArea,
                    fieldVariables,
                    services
                ),
                _ => (NoLayout.Instance, UpdateInfo.Done)
            };

            lastUpdateInfo = result.updateInfo;
            updatedLayouts = [.. updatedLayouts, result.layout.SetOffset(new Position(0, yOffset))];
            yOffset += result.layout.BoundingBox.Height;

            if(yOffset > remainingArea.Height)
            {
                break;
            }

            Model model = section.Find<Model>(layout.ModelId);
            Model next = section.Elements.Next(model.Id);
            float spaceAfter = model.CalculateSpaceAfter(result.layout.Partition, next);
            yOffset += spaceAfter;
        }

        Rectangle boudingBox = updatedLayouts
            .CalculateBoundingBox(Rectangle.Empty);

        bool isSectionFinished = updatedLayouts.Last().ModelId == section.Elements.Last().Id
            && updatedLayouts.Last().Partition.IsFinished();

        LayoutPartition lp = isSectionFinished.CalculateLayoutPartitionAfterUpdate(previousSectionLayout.Partition);

        SectionLayout updatedSection = new(
            section.Id,
            updatedLayouts,
            boudingBox,
            Borders.None,
            lp
        );

        return (updatedSection, lastUpdateInfo);
    }
}

file static class SectionOperators
{
    public static T Find<T>(this Section section, ModelId id) where T : Model =>
        section.Elements.OfType<T>().Single(e => e.Id == id);

    public static T LastOfTypeOr<T>(this Layout[] layouts, T ifNone) where T : Layout =>
        layouts switch
        {
            [.., T last] => last,
            _ => ifNone,
        };

    public static Model[] Unprocessed(this Section section, Layout[] previousLayouts) =>
        previousLayouts.Length == 0
            ? section.Elements
            : [..section.Elements.SkipFinished(previousLayouts.Last())];

    private static IEnumerable<Model> SkipFinished(this Model[] models, Layout lastLayout) =>
        models.SkipProcessed(lastLayout.ModelId, lastLayout.Partition.IsFinished());
}