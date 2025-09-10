using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Common;

internal static class ParagraphsAndTablesBuilder
{
    public static (Layout[] layouts, ProcessingInfo processingInfo) CreateParagraphAndTableLayouts(
        this Model[] unprocessedParagraphsAndTables,
        Size availableArea,
        IComposedLayout previousLayout,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        ProcessingInfo processingInfo = ProcessingInfo.Done;
        float yOffset = 0;

        Size remainingArea = availableArea;
        Layout[] layouts = [];

        foreach (Model model in unprocessedParagraphsAndTables)
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

            processingInfo = result.processingInfo;

            if (result.layout.IsNotEmpty())
            {
                remainingArea = remainingArea.DecreaseHeight(result.layout.BoundingBox.Height);
                layouts = [.. layouts, result.layout.Offset(new Position(0, yOffset))];
                yOffset += result.layout.BoundingBox.Height;
            }

            if (processingInfo is ProcessingInfo.NewPageRequired
                or ProcessingInfo.RequestDrawingArea
                or ProcessingInfo.IgnoreAndRequestDrawingArea)
            {
                break;
            }

            Model nextModel = unprocessedParagraphsAndTables.Next(model.Id);
            float spaceAfter = model.CalculateSpaceAfter(result.layout.Partition, nextModel);
            yOffset += spaceAfter;
            remainingArea = remainingArea.DecreaseHeight(spaceAfter);
        }

        return (layouts, processingInfo);
    }

    public static (Layout[] layouts, UpdateInfo UpdateInfo) UpdateParagraphAndTableLayouts(
        this Layout[] layouts,
        Model[] models,
        Size availableArea,
        IComposedLayout previousLayout,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = availableArea;
        Layout[] updatedLayouts = [];
        UpdateInfo lastUpdateInfo = UpdateInfo.Done;
        float yOffset = 0;

        foreach (Layout layout in layouts)
        {
            (Layout layout, UpdateInfo updateInfo) result = layout switch
            {
                ParagraphLayout pl => pl.Update(
                    models.Find<Paragraph>(pl.ModelId),
                    previousLayout.TryFindParagraphLayout(pl.ModelId), // try find in previous section
                    remainingArea,
                    fieldVariables,
                    services
                ),
                TableLayout tl => tl.Update(
                    models.Find<Table>(tl.ModelId),
                    previousLayout.TryFindTableLayout(tl.ModelId),
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

            Model model = models.Find<Model>(layout.ModelId);
            Model next = models.Next(model.Id);
            float spaceAfter = model.CalculateSpaceAfter(result.layout.Partition, next);
            yOffset += spaceAfter;
        }

        return (updatedLayouts, lastUpdateInfo);
    }
}
