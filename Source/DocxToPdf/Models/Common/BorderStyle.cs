using System.Drawing;

namespace Proxoft.DocxToPdf.Models.Common;

internal class BorderStyle(Pen? top, Pen? right, Pen? bottom, Pen? left)
{
    public static readonly BorderStyle NoBorder = new(null, null, null, null);

    public Pen? Top { get; } = top;

    public Pen? Right { get; } = right;

    public Pen? Bottom { get; } = bottom;

    public Pen? Left { get; } = left;
}
