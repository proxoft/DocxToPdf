using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;

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
        Layout[] layouts = [];

        Size remainingArea = availableArea
            .Clip(cell.Padding);

        ProcessingInfo cellProcessingInfo = ProcessingInfo.Done;

        float yOffset = cell.Padding.Top;
        float xOffset = cell.Padding.Left;

        foreach (Model model in unprocessed)
        {
            (Layout layout, ProcessingInfo processingInfo) result = model switch
            {
                Paragraph p => p.CreateLayout(
                    previousLayout.TryFindParagraphLayout(p.Id),
                    remainingArea,
                    fieldVariables,
                    services),
                Table t => t.CreateTableLayout(
                    previousLayout.TryFindTableLayout(t.Id),
                    remainingArea,
                    fieldVariables,
                    services),
                _ => (NoLayout.Instance, ProcessingInfo.Done)
            };

            cellProcessingInfo = result.processingInfo;
            if (result.layout.IsNotEmpty())
            {
                remainingArea = remainingArea.DecreaseHeight(result.layout.BoundingBox.Height);
                layouts = [.. layouts, result.layout.SetOffset(new Position(xOffset, yOffset))];
                yOffset += result.layout.BoundingBox.Height;
            }

            if (cellProcessingInfo is ProcessingInfo.NewPageRequired
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
           .CalculateBoundingBox(Rectangle.Empty.SetWidth(remainingArea.Width))
           .Expand(cell.Padding)
           ;

        LayoutPartition layoutPartition = cellProcessingInfo.CalculateLayoutPartition(previousLayout.Partition);

        CellLayout cellLayout = new(
            cell.Id,
            layouts,
            boudingBox,
            cell.Borders,
            cell.GridPosition,
            layoutPartition
        );

        return (cellLayout, cellProcessingInfo);
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
        Size remainingArea = availableArea
            .Clip(cell.Padding);

        float yOffset = cell.Padding.Top;
        float xOffset = cell.Padding.Left;

        Layout[] updatedLayouts = [];

        foreach (Layout layout in cellLayout.ParagraphsOrTables)
        {
            (Layout updatedLayout, UpdateInfo updateInfo) result = layout switch
            {
                ParagraphLayout pl => pl.Update(
                    cell.Find<Paragraph>(pl.ModelId),
                    previousPageCellLayout.TryFindParagraphLayout(pl.ModelId),
                    remainingArea,
                    fieldVariables,
                    services),
                TableLayout tl => tl.Update(
                    cell.Find<Table>(tl.ModelId),
                    cellLayout.TryFindTableLayout(tl.ModelId),
                    remainingArea,
                    fieldVariables,
                    services),
                _ => (NoLayout.Instance, UpdateInfo.Done)
            };

            if (result.updatedLayout != ParagraphLayout.Empty && result.updatedLayout != TableLayout.Empty)
            {
                updatedLayouts = [.. updatedLayouts, result.updatedLayout.SetOffset(new Position(xOffset, yOffset))];
            }

            if (result.updateInfo is UpdateInfo.ReconstructRequired)
            {
                break;
            }

            Model currentModel = cell.Find<Model>(layout.ModelId);
            Model nextModel = cell.ParagraphsOrTables.Next(layout.ModelId);
            float spaceAfter = currentModel.CalculateSpaceAfter(result.updatedLayout.Partition, nextModel);
            yOffset += result.updatedLayout.BoundingBox.Height + spaceAfter;
            remainingArea = remainingArea
                .DecreaseHeight(result.updatedLayout.BoundingBox.Height + spaceAfter);
        }

        Rectangle boundingBox = updatedLayouts
            .CalculateBoundingBox(Rectangle.Empty.SetWidth(remainingArea.Width))
            .Expand(cell.Padding);

        CellLayout updatedCellLayout = new(
            cell.Id,
            updatedLayouts,
            boundingBox,
            cell.Borders,
            cell.GridPosition,
            cellLayout.Partition
        );

        bool isCellFinished = updatedLayouts.IsUpdateFinished(cellLayout.ParagraphsOrTables);
        UpdateInfo updateInfo = isCellFinished
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
    public static Model[] Unprocessed(this Cell cell, Layout[] previousLayouts) =>
       previousLayouts.Length == 0
           ? cell.ParagraphsOrTables
           : [.. cell.ParagraphsOrTables.SkipFinished(previousLayouts.Last())];

    public static T Find<T>(this Cell cell, ModelId id) where T : Model =>
        cell.ParagraphsOrTables.OfType<T>().Single(m => m.Id == id);

    private static IEnumerable<Model> SkipFinished(this Model[] models, Layout lastLayout) =>
        models.SkipProcessed(lastLayout.ModelId, lastLayout.Partition.IsFinished());
}