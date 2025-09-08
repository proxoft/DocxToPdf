namespace Proxoft.DocxToPdf.Tests;

public class PageNumberTest : TestBase
{
    public PageNumberTest() : base("PageNumbers")
    {
    }

    [Fact]
    public void PageNumber()
    {
        this.Generate(nameof(PageNumber));
    }

    [Fact]
    public void PageNumberTotalPages()
    {
        this.Generate(nameof(PageNumberTotalPages));
    }

    [Fact]
    public void PageNumberTotalPagesOver10()
    {
        this.Generate(nameof(PageNumberTotalPagesOver10));
    }

    [Fact]
    public void TotalPagesReconstructParagraph()
    {
        this.Generate(nameof(TotalPagesReconstructParagraph));
    }

    [Fact]
    public void TotalPagesReconstructMultipleParagraphs()
    {
        this.Generate(nameof(TotalPagesReconstructMultipleParagraphs));
    }

    [Fact]
    public void TotalPagesWithImageTextWrapping()
    {
        this.Generate(nameof(TotalPagesWithImageTextWrapping));
    }
}
