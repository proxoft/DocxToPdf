using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Sections;

internal static class ColumnLayoutBuilder
{
    public static (ColumnLayout, ProcessingInfo) CreateSectionColumnLayout(
        this Section section,
        Size availableArea,
        ColumnLayout previousColumn,
        FieldVariables fieldVariables,
        LayoutServices services
    )
    {
        Model[] unprocessed = section.Unprocessed(previousColumn.ParagraphsOrTables);
        if (unprocessed.Length == 0)
        {
            return (ColumnLayout.None, ProcessingInfo.Done);
        }

        Layout[] layouts = [];
        Size remainingArea = availableArea;

        ProcessingInfo sectionProcessingInfo = ProcessingInfo.Done;
        float yOffset = 0;

        foreach (Model model in unprocessed)
        {
            (Layout layout, ProcessingInfo processingInfo) result = model switch
            {
                Paragraph p => p.CreateLayout(
                    previousColumn.TryFindParagraphLayout(p.Id),
                    remainingArea,
                    fieldVariables,
                    services),
                Table t => t.CreateTableLayout(
                    previousColumn.TryFindTableLayout(t.Id),
                    remainingArea,
                    fieldVariables,
                    services),
                _ => (NoLayout.Instance, ProcessingInfo.Done)
            };

            sectionProcessingInfo = result.processingInfo;

            if (result.layout.IsNotEmpty())
            {
                remainingArea = remainingArea.DecreaseHeight(result.layout.BoundingBox.Height);
                layouts = [.. layouts, result.layout.Offset(new Position(0, yOffset))];
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

        LayoutPartition lp = section.CalculateLayoutPartition(layouts);
        Rectangle boudingBox = layouts
            .CalculateBoundingBox(lp.DefaultBoundingBox(availableArea));

        ColumnLayout columnLayout = new(
            section.Id,
            layouts,
            boudingBox,
            Borders.None,
            lp
        );

        return (columnLayout, sectionProcessingInfo);
    }

    public static (ColumnLayout, UpdateInfo) Update(
        this ColumnLayout column,
        Section section,
        ColumnLayout previousColumnLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = availableArea;

        Layout[] updatedLayouts = [];
        UpdateInfo lastUpdateInfo = UpdateInfo.Done;
        float yOffset = 0;

        foreach (Layout layout in column.ParagraphsOrTables)
        {
            (Layout layout, UpdateInfo updateInfo) result = layout switch
            {
                ParagraphLayout pl => pl.Update(
                    section.Find<Paragraph>(pl.ModelId),
                    previousColumnLayout.TryFindParagraphLayout(pl.ModelId), // try find in previous section
                    remainingArea,
                    fieldVariables,
                    services
                ),
                TableLayout tl => tl.Update(
                    section.Find<Table>(tl.ModelId),
                    previousColumnLayout.TryFindTableLayout(tl.ModelId),
                    remainingArea,
                    fieldVariables,
                    services
                ),
                _ => (NoLayout.Instance, UpdateInfo.Done)
            };

            lastUpdateInfo = result.updateInfo;
            updatedLayouts = [.. updatedLayouts, result.layout.Offset(new Position(0, yOffset))];
            yOffset += result.layout.BoundingBox.Height;

            if (yOffset > remainingArea.Height)
            {
                break;
            }

            Model model = section.Find<Model>(layout.ModelId);
            Model next = section.Elements.Next(model.Id);
            float spaceAfter = model.CalculateSpaceAfter(result.layout.Partition, next);
            yOffset += spaceAfter;
        }

        LayoutPartition lp = section.CalculateLayoutPartition(updatedLayouts);
        Rectangle boudingBox = updatedLayouts
            .CalculateBoundingBox(lp.DefaultBoundingBox(availableArea));

        ColumnLayout updatedColumnLayout = new(
            section.Id,
            updatedLayouts,
            boudingBox,
            Borders.None,
            lp
        );

        return (updatedColumnLayout, lastUpdateInfo);
    }
}

file static class ColumnOperators
{
    public static T Find<T>(this Section section, ModelId id) where T : Model =>
        section.Elements.OfType<T>().Single(e => e.Id == id);

    public static Model[] Unprocessed(this Section section, Layout[] previousLayouts) =>
        previousLayouts.Length == 0
            ? section.Elements
            : [.. section.Elements.SkipFinished(previousLayouts.Last())];

    public static LayoutPartition CalculateLayoutPartition(this Section section, Layout[] layouts) =>
       section.Elements.CalculateLayoutPartition(layouts);

    public static Rectangle DefaultBoundingBox(this LayoutPartition layoutPartition, Size availableSize) =>
        Rectangle.Empty
            .SetHeight(layoutPartition.DefaultBoundingBoxHeight(availableSize))
            .SetWidth(availableSize.Width);

    private static float DefaultBoundingBoxHeight(this LayoutPartition layoutPartition, Size availableSize) =>
        layoutPartition.HasFlag(LayoutPartition.End)
            ? 0
            : availableSize.Height;


    private static IEnumerable<Model> SkipFinished(this Model[] models, Layout lastLayout) =>
        models.SkipProcessed(lastLayout.ModelId, lastLayout.Partition.IsFinished());
}
