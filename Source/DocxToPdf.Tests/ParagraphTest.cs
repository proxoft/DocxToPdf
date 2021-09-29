using Sidea.DocxToPdf.Tests;
using Xunit;

namespace DocxToPdf.Tests
{
    public class ParagraphTest : TestBase
    {
        public ParagraphTest() : base("Paragraphs")
        {
        }

        [Fact]
        public void Paragraph()
        {
            this.Generate(nameof(Paragraph));
        }


        [Fact]
        public void ParagraphAlignments()
        {
            this.Generate(nameof(ParagraphAlignments));
        }

        [Fact]
        public void ParagraphFontStyles()
        {
            this.Generate(nameof(ParagraphFontStyles));
        }

        [Fact]
        public void DefaultStyles()
        {
            this.Generate(nameof(DefaultStyles));
        }

        [Fact]
        public void ParagraphOverPage()
        {
            this.Generate(nameof(ParagraphOverPage));
        }

        [Fact]
        public void ParagraphOverPageLandscape()
        {
            this.Generate(nameof(ParagraphOverPageLandscape));
        }

        [Fact]
        public void ParagraphLineSpacing()
        {
            this.Generate(nameof(ParagraphLineSpacing));
        }

        [Fact(Skip = "not implemented feature")]
        public void VeryLongTextWithoutSpaces()
        {
            this.Generate(nameof(VeryLongTextWithoutSpaces));
        }
    }
}
