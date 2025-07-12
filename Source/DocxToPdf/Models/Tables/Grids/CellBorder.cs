namespace Proxoft.DocxToPdf.Models.Tables.Grids;

internal class CellBorder(
    BorderLine top,
    BorderLine bottom,
    BorderLine[] left,
    BorderLine[] right)
{
    public BorderLine Top { get; } = top;
    public BorderLine Bottom { get; } = bottom;
    public BorderLine[] Left { get; } = left;
    public BorderLine[] Right { get; } = right;
}
