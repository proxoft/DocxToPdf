using Xunit;

namespace Proxoft.DocxToPdf.Tests
{
    public class SectionTest : TestBase
    {
        public SectionTest() : base("Sections")
        {
            this.Options = RenderingOptions.WithDefaults(
                section: true,
                header: true,
                footer: true);
        }

        [Fact]
        public void DefaultMargins()
        {
            this.Generate(nameof(DefaultMargins));
        }

        [Fact]
        public void ResizedMargins()
        {
            this.Generate(nameof(ResizedMargins));
        }

        [Fact]
        public void Margins()
        {
            this.Generate(nameof(Margins));
        }

        [Fact]
        public void Columns()
        {
            this.Generate(nameof(Columns));
        }

        [Fact]
        public void TextOverMultipleColumns()
        {
            this.Generate(nameof(TextOverMultipleColumns));
        }

        [Fact]
        public void TextOverMultipleColumnsOverPages()
        {
            this.Generate(nameof(TextOverMultipleColumnsOverPages));
        }

        [Fact]
        public void PageOrientation()
        {
            this.Generate(nameof(PageOrientation));
        }

        [Fact]
        public void HeaderFooterOnContinuosSections()
        {
            this.Generate(nameof(HeaderFooterOnContinuosSections));
        }

        [Fact]
        public void DifferentHeaderFooterForSections()
        {
            this.Generate(nameof(DifferentHeaderFooterForSections));
        }

        [Fact]
        public void DifferentHeaderSameFooterForSections()
        {
            this.Generate(nameof(DifferentHeaderSameFooterForSections));
        }

        [Fact]
        public void SameHeaderDifferentFooterForSections()
        {
            this.Generate(nameof(SameHeaderDifferentFooterForSections));
        }

        [Fact]
        public void ContinuousSectionsWithMultipleColumns()
        {
            this.Generate(nameof(ContinuousSectionsWithMultipleColumns));
        }
    }
}
