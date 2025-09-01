using Proxoft.DocxToPdf.Tests.Tools;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using System.Linq;
using Proxoft.DocxToPdf.Tests.Assertions;

namespace Proxoft.DocxToPdf.Tests;

public class PageBreakV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
        "PageBreaks/{0}.docx",
        "PageBreaks/{0}_v2.pdf",
        new()
        {
            ParagraphBorder = new BorderStyle(new Color("FFA500"), 1, LineStyle.Solid),
            RenderParagraphCharacter = true,
        }
    );

    [Fact]
    public void PageBreak()
    {
        _executor.Convert("PageBreak", pages =>
        {
            pages.CountShouldBe(3);
        });
    }
}
