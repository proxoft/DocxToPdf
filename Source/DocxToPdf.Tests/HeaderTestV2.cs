using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class HeaderTestV2
{
    private readonly DocxToPdfExecutor _executor = new(
       "Headers/{0}.docx",
       "Headers/{0}_v2.pdf",
       new()
       {
           HeaderBorder = new BorderStyle(new Documents.Styles.Color("005659"), 1, LineStyle.Solid),
           SectionBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, LineStyle.Solid),
           // ParagraphBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
           RenderParagraphCharacter = true,
       }
    );

    [Fact]
    public void HelloWorld()
    {
        _executor.Convert("HelloWorld", pages => { });
    }
}
