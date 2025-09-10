using System;

namespace Proxoft.DocxToPdf.Layouts;

[Flags]
internal enum LayoutPartition
{
    Middle = 0,
    Start = 1,
    End = 2,
    StartEnd = 3
}

internal static class LayoutPartitionOperators
{
    public static bool IsFinished(this LayoutPartition layoutPartition) =>
        layoutPartition.HasFlag(LayoutPartition.End);
}