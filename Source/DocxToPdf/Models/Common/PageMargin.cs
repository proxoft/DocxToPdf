namespace Proxoft.DocxToPdf.Models.Common;

internal class PageMargin(double top, double right, double bottom, double left, double header, double footer) : Margin(top, right, bottom, left)
{
    public static readonly PageMargin PageNone = new(0, 0, 0, 0, 0, 0);

    public double Header { get; } = header;

    public double Footer { get; } = footer;

    public double MinimalHeaderHeight => this.Top - this.Header;

    public double FooterHeight => this.Bottom - this.Footer;

    public PageMargin WithHorizontal(double left, double right) =>
        new(this.Top, right, this.Bottom, left, this.Header, this.Footer);

    public PageMargin WithTop(double header, double top) =>
        new(top, this.Right, this.Bottom, this.Left, header, this.Footer);

    public PageMargin WithBottom(double footer, double bottom) =>
        new(this.Top, this.Right, bottom, this.Left, this.Header, footer);
}
