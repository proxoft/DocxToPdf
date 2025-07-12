using System.Linq;
using Proxoft.DocxToPdf.Extensions.Units;
using Proxoft.DocxToPdf.Models.Tables.Grids;

namespace Proxoft.DocxToPdf.Models.Tables.Builders;

internal static class GridBuilder
{
    public static Grid InitializeGrid(this Word.Table table)
    {
        double[] columnWidths = table
           .GetGridColumnWidths();

        GridRow[] rowHeights = [
            ..table
                .ChildsOfType<Word.TableRow>()
                .Select(r => r.ToGridRow())
        ];

        return new Grid(columnWidths, rowHeights);
    }

    private static double[] GetGridColumnWidths(this Word.Table table)
    {
        var grid = table.Grid();
        var columns = grid.Columns().ToArray();
        var widths = columns
            .Select(c => c.Width.ToPoint());
        return [..widths];
    }

    private static GridRow ToGridRow(this Word.TableRow row)
    {
        var trh = row
            .TableRowProperties?
            .ChildsOfType<Word.TableRowHeight>()
            .FirstOrDefault();

        var rowHeight = trh?.Val?.DxaToPoint() ?? 10;
        var rule = trh?.HeightType?.Value ?? Word.HeightRuleValues.Auto;

        return new GridRow(rowHeight, rule);
    }
}
