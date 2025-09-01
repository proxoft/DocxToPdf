using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class SectionAssertions
{
    public static TableLayout ShouldContainTable(this SectionLayout section) =>
        section.Columns[0].ShouldContainTable();

    public static ParagraphLayout ShouldContainParagraph(this SectionLayout section) =>
        section.Columns[0].ShouldContainParagraph();

    public static ColumnLayout ShouldContainColumn(this SectionLayout section, int columnIndex)
    {
        section.Columns.Length
            .Should()
            .BeGreaterThan(columnIndex);

        return section.Columns[columnIndex];
    }
}
