using Xunit;

namespace Proxoft.DocxToPdf.Tests
{
    public class FooterTest : TestBase
    {
        public FooterTest() : base("Footers")
        {
            this.Options = RenderingOptions.WithDefaults(
                footer: true);
        }

        [Fact]
        public void HelloWorld()
        {
            this.Generate(nameof(HelloWorld));
        }

        [Fact]
        public void Default()
        {
            this.Generate(nameof(Default));
        }

        [Fact]
        public void EvenOdd()
        {
            this.Generate(nameof(EvenOdd));
        }

        [Fact]
        public void FirstEvenOdd()
        {
            this.Generate(nameof(FirstEvenOdd));
        }

        [Fact]
        public void FirstEvenOddXL()
        {
            this.Generate(nameof(FirstEvenOddXL));
        }
    }
}
