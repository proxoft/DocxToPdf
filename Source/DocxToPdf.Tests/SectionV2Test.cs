using Proxoft.DocxToPdf.Documents.Styles.Borders;
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
    public void DefaultMargins()
    {
        _executor.Convert("DefaultMargins", pages =>
        {
            pages.CountShouldBe(3);
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
