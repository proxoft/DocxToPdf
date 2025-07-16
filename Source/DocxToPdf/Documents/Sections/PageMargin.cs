namespace Proxoft.DocxToPdf.Documents.Sections;

internal record PageMargin(float Top, float Right, float Bottom, float Left, float Header, float Footer)
{
    public static readonly PageMargin None = new(0, 0, 0, 0, 0, 0);

    public float HorizontalMargins() =>
        this.Left + this.Right;

    // public double MinimalHeaderHeight => this.Top - this.Header;

    // public double FooterHeight => this.Bottom - this.Footer;
}
