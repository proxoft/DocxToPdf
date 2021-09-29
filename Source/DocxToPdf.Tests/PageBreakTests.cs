
using Proxoft.DocxToPdf;
using Xunit;

namespace Sidea.DocxToPdf.Tests
{
    public class PageBreakTests : TestBase
    {
        public PageBreakTests() : base("PageBreaks")
        {
            this.Options = new RenderingOptions(
                hiddenChars: true,
                paragraphBorders: RenderingOptions.ParagraphDefault);
        }

        [Fact]
        public void PageBreak()
        {
            this.Generate(nameof(PageBreak));
        }
    }
}
