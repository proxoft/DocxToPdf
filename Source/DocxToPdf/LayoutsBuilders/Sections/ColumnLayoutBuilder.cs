using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

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

        (Layout[] layouts, ProcessingInfo processingInfo) = unprocessed.CreateParagraphAndTableLayouts(
            availableArea,
            previousColumn,
            fieldVariables,
            services
            );

        
        ColumnLayout columnLayout = layouts.ComposeColumnLayout(section, availableArea.Width);
        return (columnLayout, processingInfo);
    }

    public static (ColumnLayout, UpdateInfo) Update(
        this ColumnLayout column,
        Section section,
        ColumnLayout previousColumnLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        (Layout[] updatedLayouts, UpdateInfo updateInfo) = column.ParagraphsOrTables.UpdateParagraphAndTableLayouts(
            section.Elements,
            availableArea,
            previousColumnLayout,
            fieldVariables,
            services);

        ColumnLayout updatedColumnLayout = updatedLayouts.ComposeColumnLayout(section, availableArea.Width);
        return (updatedColumnLayout, updateInfo);
    }

    private static ColumnLayout ComposeColumnLayout(this Layout[] layouts, Section section, float width)
    {
        LayoutPartition lp = section.CalculateLayoutPartition(layouts);
        Rectangle boudingBox = layouts
            .CalculateBoundingBox(new Size(width, 0));

        ColumnLayout columnLayout = new(
           section.Id,
           layouts,
           boudingBox,
           Borders.None,
           lp
        );

        return columnLayout;
    }
}

file static class ColumnOperators
{
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
