using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class LayoutPartitionOperations
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

    public static LayoutPartition CalculateLayoutPartitionAfterUpdate(this bool allElementsDone, LayoutPartition previous)
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

    public static LayoutPartition RemoveEnd(this LayoutPartition layoutPartition) =>
        layoutPartition & ~LayoutPartition.End;
}
