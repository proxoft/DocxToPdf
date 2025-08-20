using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class ParagraphV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
        "Paragraphs/{0}.docx",
        "Paragraphs/{0}_v2.pdf",
        new()
        {
            // SectionBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
            // LineBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
            // WordBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("458976"), 1, Documents.Styles.Borders.LineStyle.Solid)
            ParagraphBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
            RenderParagraphCharacter = true,
        }
    );

    [Fact]
    public void Paragraph()
    {
        _executor.Convert("Paragraph", pages =>
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
}
