using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class LayoutingResultOperations
{
    public static LayoutPartition CalculateParagraphLayoutPartition(this ProcessingInfo processingInfo, LayoutPartition previous) =>
        processingInfo switch
        {
            ProcessingInfo.NewPageRequired when previous.IsFinished() => LayoutPartition.Start | LayoutPartition.End,
            ProcessingInfo.NewPageRequired when !previous.IsFinished() => LayoutPartition.End,
            _ => processingInfo.CalculateLayoutPartition(previous)
        };

    public static LayoutPartition CalculateLayoutPartition(this ProcessingInfo processingInfo, LayoutPartition previous)
    {
        LayoutPartition layoutPartition = LayoutPartition.Middle;
        if(previous.IsFinished())
        {
            layoutPartition |= LayoutPartition.Start;
        }

        if (processingInfo == ProcessingInfo.Done)
        {
            layoutPartition |= LayoutPartition.End;
        }

        return layoutPartition;
    }

    public static LayoutPartition CalculateLayoutPartitionAfterUpdate(this ProcessingInfo updateProcessingInfo, LayoutPartition previous, bool allElementsDone)
    {
        LayoutPartition layoutPartition = LayoutPartition.Middle;
        if(previous.IsFinished())
        {
            layoutPartition |= LayoutPartition.Start;
        }

        if(allElementsDone)
        {
            layoutPartition |= LayoutPartition.End;
        }

        return layoutPartition;
    }

    public static LayoutPartition CalculateLayoutPartition(this ResultStatus resultStatus, LayoutingResult previousResult) =>
        resultStatus switch
        {
            ResultStatus.Finished when previousResult.ModelId == ModelId.None => LayoutPartition.StartEnd,
            ResultStatus.Finished => LayoutPartition.End,
            ResultStatus.RequestDrawingArea when previousResult.ModelId == ModelId.None => LayoutPartition.Start,
            ResultStatus.RequestDrawingArea => LayoutPartition.Middle,
            _ => LayoutPartition.StartEnd
        };

    public static LayoutPartition RemoveEnd(this LayoutPartition layoutPartition) =>
        layoutPartition & ~LayoutPartition.End;

    public static CellLayoutingResult LastByOrder(this IEnumerable<CellLayoutingResult> results) =>
        results.OrderByDescending(r => r.Order).FirstOrDefault(CellLayoutingResult.None);

    public static float TotalHeight(this IEnumerable<LayoutingResult> results) =>
        results
            .SelectMany(r => r.Layouts)
            .TotalHeight();

    public static float TotalHeight(this IEnumerable<Layout> layouts) =>
        layouts.Select(l => l.BoundingBox.Height).Sum();
}
