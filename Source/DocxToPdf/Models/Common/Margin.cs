using Proxoft.DocxToPdf.Core.Structs;

namespace Proxoft.DocxToPdf.Models.Common;

public class Margin(double top, double right, double bottom, double left)
{
    public static readonly Margin None = new(0, 0, 0, 0);

    public double Top { get; } = top;

    public double Right { get; } = right;

    public double Bottom { get; } = bottom;

    public double Left { get; } = left;

    public double HorizontalMargins => this.Left + this.Right;

    public double VerticalMargins => this.Top + this.Bottom;

    public Point TopLeftReverseOffset() =>
        new(-this.Left, -this.Top);
}
