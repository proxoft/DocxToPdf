using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Tests.Assertions;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class SectionV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
       "Sections/{0}.docx",
       "Sections/{0}_v2.pdf",
       new()
       {
           SectionBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
           // ParagraphBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
           RenderParagraphCharacter = true,
       }
    );

    [Fact]
    public void Columns()
    {
        _executor.Convert("Columns", pages =>
        {
            pages.CountShouldBe(2);

            SectionLayout section = pages[0]
                .ShouldContainSectionWithId(1);

            section
                .ShouldContainColumn(0)
                .ShouldContainParagraph()
                .TextShouldStartAndEnd("Lorem", "tincidunt.");

            section
                .ShouldContainColumn(1)
                .ShouldContainParagraph()
                .TextShouldStartAndEnd("Sed", ".");

            section
                .ShouldContainColumn(2)
                .ShouldContainParagraph()
                .TextShouldStartAndEnd("Ut non", "sapien.");

            section = pages[1]
                .ShouldContainSectionWithId(1);

            section
                .ShouldContainColumn(0)
                .ShouldContainParagraph()
                .TextShouldStartAndEnd("Nam eget", ".");

            section
                .ShouldContainColumn(1)
                .ShouldContainParagraph()
                .TextShouldStartAndEnd("Cras", "efficitur.");
        });
    }

    [Fact]
    public void ContinuousSectionsWithMultipleColumns()
    {
        _executor.Convert("ContinuousSectionsWithMultipleColumns", pages => {
            pages.CountShouldBe(2);

            SectionLayout section = pages[0]
                .ShouldContainSectionWithId(1);

            #region Section 1
            section
                .ShouldHaveColumnsCount(2)
                .ShouldContainColumn(0)
                .ShouldContainParagraphAtIndex(0)
                .TextShouldEnd("First section first column");

            section
                .ShouldContainColumn(0)
                .ShouldContainParagraphAtIndex(1)
                .TextShouldEnd("Follow by column break");

            section
               .ShouldContainColumn(1)
               .ShouldContainParagraphAtIndex(0)
               .TextShouldEnd("First section second column");

            section
                .ShouldContainColumn(1)
                .ShouldContainParagraphAtIndex(1)
                .TextShouldEnd("Follow by section break");
            #endregion

            #region Section 2
            section = pages[0]
                .ShouldContainSectionWithId(2);

            section
                .ShouldHaveColumnsCount(1)
                .ShouldContainColumn(0)
                .ShouldContainParagraphAtIndex(1)
                .TextShouldEnd("single columns -------------------------------------------- ")
                ;
            #endregion

            #region Section 3
            section = pages[0]
                .ShouldContainSectionWithId(3);

            section
                .ShouldHaveColumnsCount(3)
                .ShouldContainColumn(0)
                .ShouldContainParagraphAtIndex(0)
                .TextShouldEnd("three columns: column1")
                ;

            section
                .ShouldContainColumn(1)
                .ShouldContainParagraphAtIndex(0)
                .TextShouldEnd("column2")
                ;

            section
                .ShouldContainColumn(2)
                .ShouldContainParagraphAtIndex(0)
                .TextShouldEnd("col")
                ;
            #endregion

            #region page 2 Section 4
            section = pages[1]
                .ShouldContainSectionWithId(4);

            section
                .ShouldHaveColumnsCount(1);

            #endregion
        });
    }

    [Fact]
    public void DefaultMargins()
    {
        _executor.Convert("DefaultMargins", pages =>
        {
            pages.CountShouldBe(3);
        });
    }

    [Fact]
    public void DifferentHeaderFooterForSections()
    {
        _executor.Convert("DifferentHeaderFooterForSections", pages =>
        {
            pages.CountShouldBe(2);

            pages[0]
                .ShouldHaveHeader()
                .ShouldHaveText("Header defined in the first section");

            pages[1]
                .ShouldHaveHeader()
                .ShouldHaveText("Header defined in the second section");

            pages[0]
                .ShouldHaveFooter()
                .ShouldHaveText("Footer defined in the first section");

            pages[1]
                .ShouldHaveFooter()
                .ShouldHaveText("Footer defined in the second section");
        });
    }

    [Fact]
    public void DifferentHeaderSameFooterForSections()
    {
        _executor.Convert("DifferentHeaderSameFooterForSections", pages =>
        {
            pages.CountShouldBe(2);

            pages[0]
                .ShouldHaveHeader()
                .ShouldHaveText("Header defined in the first section");

            pages[1]
                .ShouldHaveHeader()
                .ShouldHaveText("Header defined in the second section");

            pages[0]
                .ShouldHaveFooter()
                .ShouldHaveText("Footer defined in the first section");

            pages[1]
                .ShouldHaveFooter()
                .ShouldHaveText("Footer defined in the first section");
        });
    }

    [Fact]
    public void HeaderFooterOnContinuosSections()
    {
        _executor.Convert("HeaderFooterOnContinuosSections", pages =>
        {
            pages.CountShouldBe(2);
        });
    }

    [Fact]
    public void SameHeaderDifferentFooterForSections()
    {
        _executor.Convert("SameHeaderDifferentFooterForSections", pages =>
        {
            pages.CountShouldBe(2);

            pages[0]
                .ShouldHaveHeader()
                .ShouldHaveText("Header defined in the first section");

            pages[1]
                .ShouldHaveHeader()
                .ShouldHaveText("Header defined in the first section");

            pages[0]
                .ShouldHaveFooter()
                .ShouldHaveText("Footer defined in the first section");

            pages[1]
                .ShouldHaveFooter()
                .ShouldHaveText("Footer defined in the second section");
        });
    }

    [Fact]
    public void Margins()
    {
        _executor.Convert("Margins", pages =>
        {
            pages.CountShouldBe(1);
            pages[0]
                .ShouldContainSectionWithId(1);

            pages[0]
                .ShouldContainSectionWithId(2);

            pages[0]
                .ShouldContainSectionWithId(3);
        });
    }

    [Fact]
    public void ResizedMargins()
    {
        _executor.Convert("ResizedMargins", pages =>
        {
            pages.CountShouldBe(5);
        });
    }

    [Fact]
    public void PageOrientation()
    {
        _executor.Convert("PageOrientation", pages =>
        {
            pages.CountShouldBe(3);
        });
    }

    [Fact]
    public void TextOverMultipleColumns()
    {
        _executor.Convert("TextOverMultipleColumns", pages =>
        {
            pages.CountShouldBe(1);

            SectionLayout section = pages[0]
                .ShouldContainSectionWithId(1);

            _ = section.ShouldContainColumn(0)
                .ShouldContainParagraph()
                .TextShouldStart("Lorem");

            _ = section.ShouldContainColumn(1);
            _ = section.ShouldContainColumn(2)
                .ShouldContainParagraph()
                .TextShouldEnd("suscipit ");
        });
    }

    [Fact]
    public void TextOverMultipleColumnsOverPages()
    {
        _executor.Convert("TextOverMultipleColumnsOverPages", pages =>
        {
            pages.CountShouldBe(2);

            SectionLayout section = pages[0]
                .ShouldContainSectionWithId(1);

            _ = section.ShouldContainColumn(0)
                .ShouldContainParagraph()
                .TextShouldStart("Lorem");

            _ = section.ShouldContainColumn(1);
            _ = section.ShouldContainColumn(2)
                .ShouldContainParagraph()
                .TextShouldEnd(", tempus nec ");

            section = pages[1]
                .ShouldContainSectionWithId(1);

            _ = section.ShouldContainColumn(0)
                .ShouldContainParagraph()
                .TextShouldStart("dignissim");

            _ = section.ShouldContainColumn(1);
            _ = section.ShouldContainColumn(2)
                .ShouldContainParagraph()
                .TextShouldEnd("Aliquam");
        });
    }
}
