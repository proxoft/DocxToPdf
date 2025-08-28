using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class LayoutPartitionOperations
{
    public static LayoutPartition CalculateLayoutPartition(this Model[] models, Layout[] layouts, LayoutPartition ifLayoutsEmpty = LayoutPartition.Start)
    {
        if(models.Length == 0) return LayoutPartition.StartEnd;
        if(layouts.Length == 0) return ifLayoutsEmpty;

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

    public static LayoutPartition RemoveStart(this LayoutPartition layoutPartition) =>
        layoutPartition & ~LayoutPartition.Start;

    public static LayoutPartition RemoveEnd(this LayoutPartition layoutPartition) =>
        layoutPartition & ~LayoutPartition.End;
}
