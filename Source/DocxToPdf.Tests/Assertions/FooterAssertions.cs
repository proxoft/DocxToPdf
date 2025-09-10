using Proxoft.DocxToPdf.Layouts.Footers;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class FooterAssertions
{
    public static ParagraphLayout ShouldContainParagraph(this FooterLayout footerLayout)
    {
        footerLayout.ParagraphsAndTables
            .Should()
            .NotBeEmpty();

        footerLayout.ParagraphsAndTables[0]
            .Should()
            .BeOfType<ParagraphLayout>();

        return (ParagraphLayout)footerLayout.ParagraphsAndTables[0];
    }

    public static FooterLayout ShouldHaveText(this FooterLayout footerLayout, string text)
    {
        footerLayout
            .ShouldContainParagraph()
            .TextShouldStart(text);

        return footerLayout;
    }
}
