namespace Proxoft.DocxToPdf.Tests;

public class PageBreakTest : TestBase
{
    public PageBreakTest() : base("PageBreaks")
    {
        this.Options = RenderingOptions.WithDefaults(
            hiddenChars: true,
            paragraph: true);
    }

    [Fact]
    public void PageBreak()
    {
        this.Generate(nameof(PageBreak));
    }
}
