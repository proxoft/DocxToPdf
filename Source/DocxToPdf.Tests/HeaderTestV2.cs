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
           FooterBorder = new BorderStyle(new Documents.Styles.Color("005659"), 1, LineStyle.Solid),
           SectionBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, LineStyle.Solid),
           // ParagraphBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, Documents.Styles.Borders.LineStyle.Solid),
           RenderParagraphCharacter = true,
       }
    );

    [Fact]
    public void Default()
    {
        _executor.Convert("Default", pages => { });
    }

    [Fact]
    public void HelloWorld()
    {
        _executor.Convert("HelloWorld", pages => { });
    }

    [Fact]
    public void OddEven()
    {
        _executor.Convert("OddEven", pages => { });
    }

    [Fact]
    public void FirstEvenOddXXL()
    {
        _executor.Convert("FirstEvenOddXXL", pages => { });
    }

    [Fact]
    public void FirstEvenOddEvenOdd()
    {
        _executor.Convert("FirstEvenOddEvenOdd", pages => { });
    }

    [Fact]
    public void XXL()
    {
        _executor.Convert("XXL", pages => { });
    }
}
