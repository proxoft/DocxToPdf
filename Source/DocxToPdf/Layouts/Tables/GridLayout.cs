using Proxoft.DocxToPdf.Documents.Tables;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record class GridLayout(
    float[] Columns,
    RowLayout[] Rows)
{
    public static readonly GridLayout Empty = new([], []);
}

internal record RowLayout(float Height, HeightRule Rule);