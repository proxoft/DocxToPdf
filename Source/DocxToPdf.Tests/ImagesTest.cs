using Xunit;

namespace Proxoft.DocxToPdf.Tests
{
    public class ImagesTest : TestBase
    {
        public ImagesTest() : base("Images")
        {
        }

        [Fact]
        public void Image()
        {
            this.Generate(nameof(Image));
        }

        [Fact]
        public void ImageTextWrapInLine()
        {
            this.Generate(nameof(ImageTextWrapInLine));
        }

        [Fact]
        public void ImageTextWrapping()
        {
            this.Generate(nameof(ImageTextWrapping));
        }

        [Fact]
        public void ImageTextWrappingLineSpacing()
        {
            this.Generate(nameof(ImageTextWrappingLineSpacing));
        }

        [Fact]
        public void ImageResize()
        {
            this.Generate(nameof(ImageResize));
        }
    }
}
