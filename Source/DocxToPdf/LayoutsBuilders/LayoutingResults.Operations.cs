using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class LayoutingResultOperations
{
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
        layoutPartition switch
        {
            LayoutPartition.End => LayoutPartition.Middle,
            LayoutPartition.StartEnd => LayoutPartition.Start,
            _ => layoutPartition
        };

    public static CellLayoutingResult LastByOrder(this IEnumerable<CellLayoutingResult> results) =>
        results.OrderByDescending(r => r.Order).FirstOrDefault(CellLayoutingResult.None);

    public static float TotalHeight(this IEnumerable<LayoutingResult> results) =>
        results
            .SelectMany(r => r.Layouts)
            .TotalHeight();

    public static float TotalHeight(this IEnumerable<Layout> layouts) =>
        layouts.Select(l => l.BoundingBox.Height).Sum();
}
