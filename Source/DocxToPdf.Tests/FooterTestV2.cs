using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class FooterTestV2
{
    private readonly DocxToPdfExecutor _executor = new(
       "Footers/{0}.docx",
       "Footers/{0}_v2.pdf",
       new()
       {
           HeaderBorder = new BorderStyle(new Documents.Styles.Color("005659"), 1, LineStyle.Solid),
           FooterBorder = new BorderStyle(new Documents.Styles.Color("005659"), 1, LineStyle.Solid),
           SectionBorder = new BorderStyle(new Documents.Styles.Color("FF0000"), 1, LineStyle.Solid),
           RenderParagraphCharacter = true,
       }
    );

    [Fact]
    public void HelloWorld()
    {
        _executor.Convert("HelloWorld", (pages) => { });
    }

    [Fact]
    public void Default()
    {
        _executor.Convert("Default", (pages) => { });
    }

    [Fact]
    public void EvenOdd()
    {
        _executor.Convert("EvenOdd", (pages) => { });
    }

    [Fact]
    public void FirstEvenOdd()
    {
        _executor.Convert("FirstEvenOdd", (pages) => { });
    }

    [Fact]
    public void FirstEvenOddXL()
    {
        _executor.Convert("FirstEvenOddXL", (pages) => { });
    }

    [Fact]
    public void FootersForSections()
    {
        _executor.Convert("FootersForSections", pages => { });
    }
}