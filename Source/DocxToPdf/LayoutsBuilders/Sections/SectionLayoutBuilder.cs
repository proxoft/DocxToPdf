using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

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
        ColumnLayout[] columns = [];
        ProcessingInfo sectionProcessingInfo = ProcessingInfo.Done;
        ColumnLayout lastColumnLayout = previousSectionLayout.ModelId == section.Id
            ? previousSectionLayout.Columns.LastOrDefault(ColumnLayout.None)
            : ColumnLayout.None;

        float columnXOffset = 0;
        for (int columnIndex = 0; columnIndex < section.Properties.Columns.Length; columnIndex++)
        {
            Size columnArea = section.CalculateColumnArea(columnIndex, availableArea);
            (ColumnLayout column, ProcessingInfo processingInfo) = section.CreateSectionColumnLayout(
                columnArea,
                lastColumnLayout,
                fieldVariables,
                services);

            if(column.IsEmpty() && processingInfo == ProcessingInfo.Done)
            {
                break;
            }

            lastColumnLayout = column;
            if (column.IsNotEmpty())
            {
                columns = [..columns, column.Offset(new Position(columnXOffset, 0))];
            }

            sectionProcessingInfo = processingInfo;

            columnXOffset += section.WidthOccupiedByColumn(columnIndex);

            if ((sectionProcessingInfo is ProcessingInfo.RequestDrawingArea or ProcessingInfo.IgnoreAndRequestDrawingArea)
                && columnIndex < section.Properties.Columns.Length - 1)
            {
                // proceed to next column
                continue;
            }

            if (sectionProcessingInfo is ProcessingInfo.NewPageRequired
                or ProcessingInfo.RequestDrawingArea
                or ProcessingInfo.IgnoreAndRequestDrawingArea)
            {
                break;
            }
        }

        SectionLayout sectionLayout = columns.ComposeSectionLayout(section);
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
        ColumnLayout[] updatedColumns = [];
        ColumnLayout previousColumnLayout = previousSectionLayout.ModelId == section.Id
            ? previousSectionLayout.Columns.LastOrDefault(ColumnLayout.None)
            : ColumnLayout.None; 

        float columnXOffset = 0;
        int columnIndex = 0;
        UpdateInfo updateInfo = UpdateInfo.Done;
        foreach (ColumnLayout cl in sectionLayout.Columns)
        {
            Size columnArea = section.CalculateColumnArea(columnIndex, availableArea);

            (ColumnLayout updatedColumn, updateInfo) = cl.Update(
                section,
                previousColumnLayout,
                columnArea,
                fieldVariables,
                services);

            updatedColumns = [.. updatedColumns, updatedColumn.ResetOffset().Offset(new Position(columnXOffset, 0))];
            columnXOffset += section.WidthOccupiedByColumn(columnIndex);
            columnIndex++;
            previousColumnLayout = updatedColumn;
            if(updateInfo == UpdateInfo.ReconstructRequired)
            {
                break;
            }
        }

        SectionLayout updatedLayout = updatedColumns.ComposeSectionLayout(section);
        return (updatedLayout, updateInfo);
    }
}

file static class SectionOperators2
{
    public static LayoutPartition CalculateLayoutPartition(this ColumnLayout[] columns)
    {
        if (columns.Length == 0) return LayoutPartition.StartEnd;
        LayoutPartition layoutPartition = LayoutPartition.StartEnd;
        if (!columns[0].Partition.HasFlag(LayoutPartition.Start)) layoutPartition = layoutPartition.RemoveStart();
        if (!columns[^1].Partition.HasFlag(LayoutPartition.End)) layoutPartition = layoutPartition.RemoveEnd();
        return layoutPartition;
    }

    public static SectionLayout ComposeSectionLayout(this ColumnLayout[] columns, Section section)
    {
        Rectangle boudingBox = columns
            .CalculateBoundingBox(Rectangle.Empty)
            .MoveXBy(section.Properties.PageConfiguration.Margin.Left);

        LayoutPartition layoutPartition = columns.CalculateLayoutPartition();
        SectionLayout sectionLayout = new(
            section.Id,
            columns,
            boudingBox,
            Borders.None,
            layoutPartition
        );

        return sectionLayout;
    }

    public static Size CalculateSectionArea(this Section section, Size pageArea) =>
        pageArea.DecreaseWidth(section.Properties.PageConfiguration.Margin.HorizontalMargins());

    public static Size CalculateColumnArea(this Section section, int columnIndex, Size sectionAvailableSpace) =>
        new(section.Properties.Columns[columnIndex].Width, sectionAvailableSpace.Height);

    public static float WidthOccupiedByColumn(this Section section, int columnIndex) =>
        section.Properties.Columns[columnIndex].Width + section.Properties.Columns[columnIndex].SpaceAfter;
}


file static class SectionOperators
{

}