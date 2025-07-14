using System.Linq;
using Proxoft.DocxToPdf.Extensions;
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
        Word.TableGrid grid = table.Grid();
        Word.GridColumn[] columns = [..grid.Columns()];
        System.Collections.Generic.IEnumerable<double> widths = columns
            .Select(c => c.Width.ToPoint());
        return [..widths];
    }

    private static GridRow ToGridRow(this Word.TableRow row)
    {
        Word.TableRowHeight? trh = row
            .TableRowProperties?
            .ChildsOfType<Word.TableRowHeight>()
            .FirstOrDefault();

        double rowHeight = trh?.Val?.DxaToPoint() ?? 10;
        Word.HeightRuleValues rule = trh?.HeightType?.Value ?? Word.HeightRuleValues.Auto;

        return new GridRow(rowHeight, rule);
    }
}
