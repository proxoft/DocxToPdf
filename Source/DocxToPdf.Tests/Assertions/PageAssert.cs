using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class PageAssert
{
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
