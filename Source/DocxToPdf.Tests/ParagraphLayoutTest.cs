using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsRendering;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class ParagraphLayoutTest
{
    private readonly RenderOptions _options = new()
    {
        WordBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("000000"), 1, Documents.Styles.Borders.LineStyle.Solid)
    };

    [Fact]
    public void Paragraph()
    {
        PageLayout[] pages = "Paragraphs/Paragraph.docx".ReadAndLayoutDocument();

        pages
            .Should()
            .NotBeEmpty();

        "Paragraphs/v2_Paragraph.pdf".RenderAndSave(pages, _options);
    }

    [Fact]
    public void ParagraphOverPage()
    {
        PageLayout[] pages = "Paragraphs/ParagraphOverPage.docx".ReadAndLayoutDocument();

        pages
            .Should()
            .NotBeEmpty();


        "Paragraphs/v2_ParagraphOverPage.pdf".RenderAndSave(pages, _options);
    }

    [Fact]
    public void ParagraphOverPageSimple()
    {
        PageLayout[] pages = "Paragraphs/ParagraphOverPageSimple.docx".ReadAndLayoutDocument();

        pages
            .Should()
            .NotBeEmpty();

        "Paragraphs/v2_ParagraphOverPageSimple.pdf".RenderAndSave(pages, _options);
    }
}
