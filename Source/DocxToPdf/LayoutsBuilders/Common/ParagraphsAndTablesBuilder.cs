using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
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
}
