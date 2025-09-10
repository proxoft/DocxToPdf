using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Tests.Assertions;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class ParagraphV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
        "Paragraphs/{0}.docx",
        "Paragraphs/{0}_v2.pdf",
        new()
        {
            // PageContentBorder = new BorderStyle(new Documents.Styles.Color("458976"), 1, Documents.Styles.Borders.LineStyle.Solid),
            // SectionBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
            LineBorder = new BorderStyle(new Documents.Styles.Color("00FF00"), 1, Documents.Styles.Borders.LineStyle.Solid),
            //ParagraphBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
            RenderWhitespaceCharacters = true,
            RenderParagraphCharacter = true,
        }
    );

    [Fact]
    public void Paragraph()
    {
        _executor.Convert("Paragraph", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void DefaultStyles()
    {
        _executor.Convert("DefaultStyles", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ParagraphSpacing()
    {
        _executor.Convert("ParagraphSpacing", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ParagraphLineSpacing()
    {
        _executor.Convert("ParagraphLineSpacing", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ParagraphOverPage()
    {
        _executor.Convert("ParagraphOverPage", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ParagraphOverPageSimple()
    {
        _executor.Convert("ParagraphOverPageSimple", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ParagraphFontStyles()
    {
        _executor.Convert("ParagraphFontStyles", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ParagraphAlignments()
    {
        _executor.Convert("ParagraphAlignments", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }
}
