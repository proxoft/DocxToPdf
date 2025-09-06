using Proxoft.DocxToPdf.Tests.Tools;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Borders;

namespace Proxoft.DocxToPdf.Tests;

public class PageNumberV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
        "PageNumbers/{0}.docx",
        "PageNumbers/{0}_v2.pdf",
        new()
        {
            HeaderBorder = new BorderStyle(new Color("00A5B1"), 1, LineStyle.Solid),
            FooterBorder = new BorderStyle(new Color("00A512"), 1, LineStyle.Solid),
            ParagraphBorder = new BorderStyle(new Color("FFA500"), 1, LineStyle.Solid),
            RenderParagraphCharacter = true,
        }
    );

    [Fact]
    public void PageNumber()
    {
        _executor.Convert("PageNumber", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void PageNumberTotalPages()
    {
        _executor.Convert("PageNumberTotalPages", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void PageNumberTotalPagesOver10()
    {
        _executor.Convert("PageNumberTotalPagesOver10", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void TotalPagesReconstructParagraph()
    {
        _executor.Convert("TotalPagesReconstructParagraph", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void TotalPagesReconstructMultipleParagraphs()
    {
        _executor.Convert("TotalPagesReconstructMultipleParagraphs", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void TotalPagesReconstructTable()
    {
        _executor.Convert("TotalPagesReconstructTable", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void TotalPagesReconstructTableResizeParagraph()
    {
        _executor.Convert("TotalPagesReconstructTableResizeParagraph", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }
}
