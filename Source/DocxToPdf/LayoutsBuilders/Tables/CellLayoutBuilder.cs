using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class CellLayoutBuilder
{
    public static (CellLayout, ProcessingInfo) CreateLayout(
        this Cell cell,
        Size availableArea,
        FieldVariables fieldVariables,
        CellLayout previousLayout,
        LayoutServices services)
    {
        Model[] unprocessed = cell.Unprocessed(previousLayout.ParagraphsOrTables);
        

        Padding effectivePadding = cell.Padding.UpdatePaddingByPartitioning(previousLayout);
        Size remainingArea = availableArea
            .Clip(effectivePadding);

        (Layout[] layouts, ProcessingInfo processingInfo) = unprocessed.CreateParagraphAndTableLayouts(
            remainingArea,
            previousLayout,
            fieldVariables,
            services);

        Position offset = new(effectivePadding.Left, effectivePadding.Top);
        layouts = [..layouts.Select(l => l.Offset(offset))];

        Rectangle boundingBox = layouts
           .CalculateBoundingBox(new Size(remainingArea.Width, 0))
           .Expand(effectivePadding)
           ;

        LayoutPartition layoutPartition = cell.CalculateLayoutPartition(layouts, previousLayout);
        CellLayout cellLayout = new(
            cell.Id,
            layouts,
            boundingBox,
            cell.Borders,
            cell.GridPosition,
            layoutPartition
        );

        return (cellLayout, processingInfo);
    }

    public static (CellLayout, UpdateInfo) Update(
        this CellLayout cellLayout,
        Cell cell,
        Size availableArea,
        FieldVariables fieldVariables,
        CellLayout previousPageCellLayout,
        LayoutServices services
    )
    {
        Padding effectivePadding = cell.Padding.UpdatePaddingByPartitioning(previousPageCellLayout);
        Size cellContentArea = availableArea
            .Clip(effectivePadding);

        (Layout[] updatedLayouts, UpdateInfo updateInfo) = cellLayout.ParagraphsOrTables.UpdateParagraphAndTableLayouts(
            cell.ParagraphsOrTables,
            cellContentArea,
            previousPageCellLayout,
            fieldVariables,
            services
        );

        Position offset = new(effectivePadding.Left, effectivePadding.Top);
        updatedLayouts = [.. updatedLayouts.Select(l => l.Offset(offset))];

        Rectangle boundingBox = updatedLayouts
           .CalculateBoundingBox(new Size(cellContentArea.Width, 0))
           .Expand(effectivePadding)
           ;

        LayoutPartition layoutPartition = cell.CalculateLayoutPartition(
            updatedLayouts,
            previousPageCellLayout
        );

        CellLayout updatedCellLayout = new(
            cell.Id,
            updatedLayouts,
            boundingBox,
            cell.Borders,
            cell.GridPosition,
            layoutPartition
        );

        bool isCellFinished = updatedLayouts.IsUpdateFinished(cellLayout.ParagraphsOrTables);
        updateInfo = isCellFinished
            ? UpdateInfo.Done
            : UpdateInfo.ReconstructRequired;

        return (updatedCellLayout, updateInfo);
    }

    public static CellLayout[] AlignCellHeights(this CellLayout[] cellLayouts, GridLayout grid) =>
        [..cellLayouts.Select(cl => cl.AlignHeight(grid))];

    public static CellLayout[] AlignLayoutPartitions(this CellLayout[] cellLayouts)
    {
        if(cellLayouts.Length <= 1)
        {
            return cellLayouts;
        }

        CellLayout last = cellLayouts[^1];
        CellLayout[] others = [.. cellLayouts.SkipLast(1)];

        return last.Partition.IsFinished()
            ? last.AlignLastWithOthers(others)
            : last.AlignOthersWithLast(others);
    }

    private static CellLayout[] AlignLastWithOthers(this CellLayout last, CellLayout[] others)
    {
        bool existsUnfinishedCell = others
                .Where(c => c.GridPosition.ContainsRowIndex(last.GridPosition.BottomRow()))
                .Any(c => !c.Partition.IsFinished());

        CellLayout updated = existsUnfinishedCell
            ? last.ForceLayoutPartitionNotFinished()
            : last;

        return [..others, updated];
    }

    private static CellLayout[] AlignOthersWithLast(this CellLayout last, CellLayout[] others) =>
        [
            ..others
                .Select(cl => cl.GridPosition.BottomRow() != last.GridPosition.BottomRow()
                    ? cl
                    : cl.ForceLayoutPartitionNotFinished()),
            last
        ];

    private static CellLayout AlignHeight(this CellLayout cellLayout, GridLayout grid)
    {
        float height = grid.CalculateCellAvailableHeight(cellLayout.GridPosition);
        if (cellLayout.BoundingBox.Height >= height)
        {
            return cellLayout;
        }

        return cellLayout with
        {
            BoundingBox = cellLayout.BoundingBox.SetHeight(height)
        };
    }

    private static CellLayout ForceLayoutPartitionNotFinished(this CellLayout cellLayout) =>
        cellLayout with
        {
            Partition = cellLayout.Partition.RemoveEnd()
        };
}

file static class CellOperators
{
    public static Padding UpdatePaddingByPartitioning(this Padding padding, CellLayout previousLayout) =>
        !previousLayout.Partition.HasFlag(LayoutPartition.End)
            ? padding with
            {
                Top = 0
            }
            : padding;

    public static LayoutPartition CalculateLayoutPartition(
        this Cell cell,
        Layout[] layouts,
        CellLayout previousLayout)
    {
        LayoutPartition ifLayoutsEmpty = previousLayout.ModelId == cell.Id
            ? LayoutPartition.End
            : LayoutPartition.Start;

        LayoutPartition layoutPartition = cell.ParagraphsOrTables.CalculateLayoutPartition(layouts, ifLayoutsEmpty);
        if(previousLayout.ModelId == cell.Id)
        {
            layoutPartition = layoutPartition.RemoveStart();
        }

        return layoutPartition;
    }

    public static Model[] Unprocessed(this Cell cell, Layout[] previousLayouts) =>
       previousLayouts.Length == 0
           ? cell.ParagraphsOrTables
           : [.. cell.ParagraphsOrTables.SkipFinished(previousLayouts.Last())];

    private static IEnumerable<Model> SkipFinished(this Model[] models, Layout lastLayout) =>
        models.SkipProcessed(lastLayout.ModelId, lastLayout.Partition.IsFinished());
}