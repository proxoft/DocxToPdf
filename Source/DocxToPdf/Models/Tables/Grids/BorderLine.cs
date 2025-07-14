using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Core.Pages;
using Drawing = System.Drawing;

namespace Proxoft.DocxToPdf.Models.Tables.Grids;

internal class BorderLine(PageNumber pageNumber, Point start, Point end)
{
    public PageNumber PageNumber { get; } = pageNumber;

    public Point Start { get; } = start;

    public Point End { get; } = end;

    public Line ToLine(Drawing.Pen? pen) =>
        new(this.Start, this.End, pen ?? new Drawing.Pen(Drawing.Brushes.Transparent));
}
