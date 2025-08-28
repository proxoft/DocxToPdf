using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class LayoutPartitionOperations
{
    public static LayoutPartition CalculateLayoutPartition(this Model[] models, Layout[] layouts)
    {
        if(models.Length == 0) return LayoutPartition.StartEnd;
        if(layouts.Length == 0) return LayoutPartition.Start;

        LayoutPartition layoutPartition = LayoutPartition.Middle;
        if(layouts[0].ModelId == models[0].Id
            && layouts[0].Partition.HasFlag(LayoutPartition.Start))
        {
            layoutPartition |= LayoutPartition.Start;
        }

        if (layouts[^1].ModelId == models[^1].Id
            && layouts[^1].Partition.HasFlag(LayoutPartition.End))
        {
            layoutPartition |= LayoutPartition.End;
        }

        return layoutPartition;
    }

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
