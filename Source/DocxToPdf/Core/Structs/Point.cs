using System.Diagnostics;

namespace Proxoft.DocxToPdf.Core.Structs;

[DebuggerDisplay("{X},{Y}")]
public class Point(double x, double y)
{
    public static readonly Point Zero = new(0, 0);

    public double X { get; } = x;

    public double Y { get; } = y;

    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.X + p2.X, p1.Y + p2.Y);
    }
}
