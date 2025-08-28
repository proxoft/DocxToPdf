using System.Linq;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class CellAssertions
{
    public static CellLayout ShouldBeComplete(this CellLayout cell) =>
        cell
            .ShouldBeStart()
            .ShouldBeEnd();

    public static CellLayout ShouldStartAndEndWithText(this CellLayout cell, string startText, string endText)
    {
        ParagraphLayout _ = cell
            .ShouldContainParagraphOnPosition(0)
            .TextShouldStart(startText);

        cell.ParagraphsOrTables
            .Last()
            .Should()
            .BeOfType<ParagraphLayout>();

        _ = ((ParagraphLayout)cell.ParagraphsOrTables.Last())
            .TextShouldEnd(endText);

        return cell;
    }

    public static CellLayout ShouldBeStart(this CellLayout cell, bool only = false)
    {
        cell.Partition.HasFlag(Layouts.LayoutPartition.Start)
            .Should()
            .BeTrue();

        if (only)
        {
            cell.ShouldNotBeEnd();
        }

        return cell;
    }

    public static CellLayout ShouldNotBeStart(this CellLayout cell)
    {
        cell.Partition
            .Should()
            .NotHaveFlag(Layouts.LayoutPartition.Start);

        return cell;
    }

    public static CellLayout ShouldBeEnd(this CellLayout cell, bool only = false)
    {
        cell.Partition
            .Should()
            .HaveFlag(Layouts.LayoutPartition.End);

        if(only)
        {
            cell.ShouldNotBeStart();
        }

        return cell;
    }

    public static CellLayout ShouldNotBeEnd(this CellLayout cell)
    {
        cell.Partition
           .Should()
           .NotHaveFlag(Layouts.LayoutPartition.End);

        return cell;
    }

    public static CellLayout ShouldBeEmpty(this CellLayout cell)
    {
        cell.ParagraphsOrTables
            .Should()
            .BeEmpty();
        return cell;
    }

    public static ParagraphLayout CellShouldContainOneParagraph(this CellLayout cell)
    {
        cell.ParagraphsOrTables
            .OfType<ParagraphLayout>()
            .Should()
            .NotBeEmpty();

        return cell.ParagraphsOrTables
            .OfType<ParagraphLayout>()
            .Single();
    }

    public static ParagraphLayout ShouldContainParagraphOnPosition(this CellLayout cell, int position)
    {
        cell.ParagraphsOrTables.Length
            .Should()
            .BeGreaterThan(position);

        cell.ParagraphsOrTables[position]
            .Should()
            .BeOfType<ParagraphLayout>();

        return (ParagraphLayout)cell.ParagraphsOrTables[position];
    }
}
