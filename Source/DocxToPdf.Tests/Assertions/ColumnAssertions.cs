using System.Linq;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class ColumnAssertions
{
    public static TableLayout ShouldContainTable(this ColumnLayout sectionColumn)
    {
        sectionColumn.ParagraphsOrTables
            .Should()
            .NotBeEmpty();

        sectionColumn.ParagraphsOrTables[0]
            .Should()
            .BeOfType<TableLayout>();

        return (TableLayout)sectionColumn.ParagraphsOrTables[0];
    }

    public static ParagraphLayout ShouldContainParagraph(this ColumnLayout sectionColumn)
    {
        sectionColumn.ParagraphsOrTables
            .Should()
            .NotBeEmpty();

        sectionColumn.ParagraphsOrTables[0]
            .Should()
            .BeOfType<ParagraphLayout>();

        return (ParagraphLayout)sectionColumn.ParagraphsOrTables[0];
    }

    public static ParagraphLayout ShouldContainParagraphAtIndex(this ColumnLayout sectionColumn, int order)
    {
        sectionColumn.ParagraphsOrTables
            .Should()
            .NotBeEmpty();

        sectionColumn.ParagraphsOrTables
            .OfType<ParagraphLayout>()
            .Count()
            .Should()
            .BeGreaterThan(order);
            ;

        return sectionColumn.ParagraphsOrTables
            .OfType<ParagraphLayout>()
            .ElementAt(order);
    }
}
