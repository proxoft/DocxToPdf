using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Tables;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record class GridLayout(
    ModelId ModelId,
    float[] Columns,
    RowLayout[] Rows)
{
    public static readonly GridLayout Empty = new(ModelId.None, [], []);
}

internal record RowLayout(int Row, float Height, HeightRule Rule);