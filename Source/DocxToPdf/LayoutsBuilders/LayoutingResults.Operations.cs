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
}
