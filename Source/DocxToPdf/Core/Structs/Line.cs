using D = System.Drawing;

namespace Proxoft.DocxToPdf.Core.Structs;

internal class Line
{
    public Line(
        Point start,
        Point end,
        D.Pen? pen = null)
    {
        this.Start = start;
        this.End = end;
        this.Pen = pen;
    }

    public Point Start { get; }
    public Point End { get; }
    public D.Pen? Pen { get; }
}
