using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class SectionAssert
{
    public static TableLayout ShouldContainTable(this SectionLayout section)
    {
        section.Layouts
            .Should()
            .NotBeEmpty();

        section.Layouts[0]
            .Should()
            .BeOfType<TableLayout>();

        return (TableLayout)section.Layouts[0];
    }

    public static ParagraphLayout ShouldContainParagraph(this SectionLayout section)
    {
        section.Layouts
            .Should()
            .NotBeEmpty();

        section.Layouts[0]
            .Should()
            .BeOfType<ParagraphLayout>();

        return (ParagraphLayout)section.Layouts[0];
    }
}
