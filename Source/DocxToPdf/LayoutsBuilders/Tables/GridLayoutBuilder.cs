using System.Linq;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class GridLayoutBuilder
{
    public static GridLayout CreateGridLayout(this Grid grid) =>
        new(
            [..grid.ColumnWidths],
            [.. grid.RowHeights.Select(r => r.Height)]
        );
}
