using Xunit;

namespace Proxoft.DocxToPdf.Tests
{
    public class PageBreakTests : TestBase
    {
        public PageBreakTests() : base("PageBreaks")
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
}
