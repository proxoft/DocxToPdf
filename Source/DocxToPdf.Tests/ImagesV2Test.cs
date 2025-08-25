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
            ParagraphBorder = new BorderStyle(new Color("FFA500"), 1, LineStyle.Solid),
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
}
