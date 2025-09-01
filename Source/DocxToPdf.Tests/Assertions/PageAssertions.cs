using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Sections;
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

    public static PageLayout ShouldContainSections(this PageLayout page, int sectionCount)
    {
        page.PageContent.Sections
            .Length
            .Should()
            .Be(sectionCount);

        return page;
    }

    public static SectionLayout ShouldContainSectionWithId(this PageLayout page, int sectionId)
    {
        ModelId modelId = sectionId.AsSectionId();
        page.PageContent
            .Sections
            .Any(s => s.ModelId == modelId)
            .Should()
            .BeTrue();

        return page.PageContent.Sections.Single(s => s.ModelId == modelId);
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
