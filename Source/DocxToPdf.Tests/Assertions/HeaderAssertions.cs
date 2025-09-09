using Proxoft.DocxToPdf.Layouts.Headers;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class HeaderAssertions
{
    public static ParagraphLayout ShouldContainParagraph(this HeaderLayout headerLayout)
    {
        headerLayout.ParagraphsAndTables
            .Should()
            .NotBeEmpty();

        headerLayout.ParagraphsAndTables[0]
            .Should()
            .BeOfType<ParagraphLayout>();

        return (ParagraphLayout)headerLayout.ParagraphsAndTables[0];
    }

    public static HeaderLayout ShouldHaveText(this HeaderLayout headerLayout, string text)
    {
        headerLayout
            .ShouldContainParagraph()
            .TextShouldStart(text);

        return headerLayout;
    }
}
