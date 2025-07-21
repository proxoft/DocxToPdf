using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Extensions;

namespace Proxoft.DocxToPdf.Builders.Tables;

internal static class TableBuilder
{
    private const float _defaultRowHeight = 10;

    public static Table ToTable(this Word.Table table, BuilderServices services)
    {
        ModelId tableId = services.IdFactory.NextTableId();
        Grid grid = table.CreateTableGrid();
        Cell[] cells = [..table.CreateCells(services)];

        return new Table(
            tableId,
            cells,
            grid,
            new Borders(BorderStyle.None, BorderStyle.None, BorderStyle.None, BorderStyle.None)
        );
    }

    private static Grid CreateTableGrid(this Word.Table table)
    {
        float[] columnWidths = table.GetGridColumnWidths();
        GridRow[] rows = [..table
            .ChildsOfType<Word.TableRow>()
            .Select(row => row.ToGridRow())
        ];

        return new Grid(columnWidths, rows);
    }

    private static float[] GetGridColumnWidths(this Word.Table table)
    {
        Word.TableGrid grid = table.Grid();
        Word.GridColumn[] columns = [.. grid.Columns()];

        float[] widths = [..columns
            .Select(c => c.Width.ToPoint())
        ];

        return widths;
    }

    private static GridRow ToGridRow(this Word.TableRow row)
    {
        Word.TableRowHeight? trh = row
            .TableRowProperties?
            .ChildsOfType<Word.TableRowHeight>()
            .FirstOrDefault();

        float rowHeight = trh?.Val?.DxaToPoint() ?? _defaultRowHeight;
        Word.HeightRuleValues rule = trh?.HeightType?.Value ?? Word.HeightRuleValues.Auto;
        HeightRule heightRule = HeightRule.Auto;
        if (rule == Word.HeightRuleValues.Exact) heightRule = HeightRule.Exact;
        if (rule == Word.HeightRuleValues.AtLeast) heightRule = HeightRule.AtLeast;

        return new GridRow(rowHeight, heightRule);
    }
}
