namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record class GridLayout(
    float[] Columns,
    float[] Rows)
{
    public static readonly GridLayout Empty = new([], []);
}
