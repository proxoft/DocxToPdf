namespace Proxoft.DocxToPdf.Models.Common;

internal class HorizontalSpace(double x, double width)
{
    public double X { get; } = x;

    public double Width { get; } = width;

    public double RightX => this.X + this.Width;
}
