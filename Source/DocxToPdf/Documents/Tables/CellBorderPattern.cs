using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;

namespace Proxoft.DocxToPdf.Documents.Tables;

internal record CellBorderPattern(
    BorderStyle Left,
    BorderStyle Top,
    BorderStyle Right,
    BorderStyle Bottom,
    BorderStyle Horizontal,
    BorderStyle Vertical
) : Borders(Left, Top, Right, Bottom)
{
    public static readonly CellBorderPattern Default = new(BorderStyle.Default, BorderStyle.Default, BorderStyle.Default, BorderStyle.Default, BorderStyle.Default, BorderStyle.Default);
}
