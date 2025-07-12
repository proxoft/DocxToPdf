using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Core.Pages;
using Drawing = System.Drawing;

namespace Proxoft.DocxToPdf.Models.Tables.Grids;

internal class BorderLine
{
    public BorderLine(PageNumber pageNumber, Point start, Point end)
    {
        this.PageNumber = pageNumber;
        this.Start = start;
        this.End = end;
    }

    public PageNumber PageNumber { get; }

    public Point Start { get; }

    public Point End { get; }

    public Line ToLine(Drawing.Pen pen) =>
        new(this.Start, this.End, pen ?? new Drawing.Pen(Drawing.Brushes.Transparent));
}
