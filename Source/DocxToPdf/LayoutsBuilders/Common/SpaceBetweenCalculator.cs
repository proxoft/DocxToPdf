using System;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Common;

internal static class SpaceBetweenCalculator
{
    public static float CalculateSpaceAfter(this Model model, LayoutPartition layoutPartition, Model next)
    {
        if (!layoutPartition.HasFlag(LayoutPartition.End)) return 0;
        float minSpaceAfter = model.SpaceAfter();
        float minSpaceBefore = next.SpaceBefore();

        return Math.Max(minSpaceAfter, minSpaceBefore);
    }

    private static float SpaceBefore(this Model model) =>
        model switch
        {
            Paragraph p => p.Style.ParagraphSpacing.Before,
            _ => 0
        };

    private static float SpaceAfter(this Model model) =>
        model switch
        {
            Paragraph p => p.Style.ParagraphSpacing.After,
            _ => 0
        };
}
