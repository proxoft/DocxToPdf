using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class ImagesV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
        "Images/{0}.docx",
        "Images/{0}_v2.pdf",
        new()
        {
            // ParagraphBorder = new BorderStyle(new Color("FFA500"), 1, LineStyle.Solid),
            LineBorder = new BorderStyle(new Color("FFA500"), 1, LineStyle.Solid),
            RenderWhitespaceCharacters = true,
            RenderParagraphCharacter = true,
        }
    );

    [Fact]
    public void Image()
    {
        _executor.Convert("Image", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ImageResize()
    {
        _executor.Convert("ImageResize", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ImageTextWrapInLine()
    {
        _executor.Convert("ImageTextWrapInLine", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ImageTextWrapping()
    {
        _executor.Convert("ImageTextWrapping", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ImageTextWrapping2()
    {
        _executor.Convert("ImageTextWrapping2", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void ImageTextWrappingLineSpacing()
    {
        _executor.Convert("ImageTextWrappingLineSpacing", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }
}
