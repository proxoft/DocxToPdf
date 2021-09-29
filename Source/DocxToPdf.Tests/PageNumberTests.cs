using Xunit;

namespace Proxoft.DocxToPdf.Tests
{
    public class PageNumberTests : TestBase
    {
        public PageNumberTests() : base("PageNumbers")
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
        public void PageNumberTotalPages_Over10()
        {
            this.Generate(nameof(PageNumberTotalPages_Over10));
        }

        [Fact]
        public void TotalPages_ReconstructParagraph()
        {
            this.Generate(nameof(TotalPages_ReconstructParagraph));
        }

        [Fact]
        public void TotalPages_ReconstructMultipleParagraphs()
        {
            this.Generate(nameof(TotalPages_ReconstructMultipleParagraphs));
        }
    }
}
