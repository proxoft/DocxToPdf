using System.Linq;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class CellAssertions
{
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
}
