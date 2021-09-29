using Xunit;

namespace Proxoft.DocxToPdf.Tests
{
    public class HeaderTest : TestBase
    {
        public HeaderTest() : base("Headers")
        {
            this.Options = RenderingOptions.WithDefaults(header: true);
        }

        [Fact]
        public void HelloWorld()
        {
            this.Generate(nameof(HelloWorld));
        }

        [Fact]
        public void XXL()
        {
            this.Generate(nameof(XXL));
        }

        [Fact]
        public void Default()
        {
            this.Generate(nameof(Default));
        }

        [Fact]
        public void OddEven()
        {
            this.Generate(nameof(OddEven));
        }

        [Fact]
        public void FirstEvenOddEvenOdd()
        {
            this.Generate(nameof(FirstEvenOddEvenOdd));
        }

        [Fact]
        public void FirstEvenOddXXL()
        {
            this.Generate(nameof(FirstEvenOddXXL));
        }
    }
}
