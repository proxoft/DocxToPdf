using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class PageAssertions
{
    public static PageLayout[] CountShouldBe(this PageLayout[] pages, int count)
    {
        pages.Length
            .Should()
            .Be(count);
        return pages;
    }

    public static TableLayout PageShouldContainSingleTable(this PageLayout page)
    {
        page.PageContent
            .Sections
            .Should()
            .NotBeEmpty();

        return page.PageContent
            .Sections[0]
            .ShouldContainTable();
    }

    public static ParagraphLayout ShouldContainSingleParagraph(this PageLayout page)
    {
        page.PageContent
            .Sections
            .Should()
            .NotBeEmpty();

        return page.PageContent
            .Sections[0]
            .ShouldContainParagraph();
    }
}
