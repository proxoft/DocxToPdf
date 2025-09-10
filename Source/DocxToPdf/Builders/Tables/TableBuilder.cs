using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Extensions;

namespace Proxoft.DocxToPdf.Builders.Tables;

internal static class TableBuilder
{
    private const float _defaultRowHeight = 10;

    public static Table ToTable(this Word.Table table, BuilderServices services)
    {
        BuilderServices forTable = services.ForTable(table.Properties());
        return table.ToTableInternal(forTable);
    }

    private static Table ToTableInternal(this Word.Table table, BuilderServices services)
    {
        ModelId tableId = services.IdFactory.NextTableId();
        Grid grid = table.CreateTableGrid();
        CellBorderPattern cellBorderPattern = table.CreateCellBorderPattern();
        Cell[] cells = [.. table.CreateCells(cellBorderPattern, services)];
        Alignment alignment = table.GetAlignment();

        return new Table(
            tableId,
            cells,
            alignment,
            grid,
            cellBorderPattern,
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

    private static CellBorderPattern CreateCellBorderPattern(this Word.Table table)
    {
        Word.TableProperties tableProps = table.ChildElements.OfType<Word.TableProperties>().Single();
        return tableProps.TableBorders.GetBorder();
    }

    private static CellBorderPattern GetBorder(this Word.TableBorders? borders)
    {
        if (borders is null)
        {
            return CellBorderPattern.Default;
        }

        BorderStyle top = borders.TopBorder.ToBorderStyle();
        BorderStyle right = borders.RightBorder.ToBorderStyle();
        BorderStyle bottom = borders.BottomBorder.ToBorderStyle();
        BorderStyle left = borders.LeftBorder.ToBorderStyle();

        BorderStyle insideH = borders.InsideHorizontalBorder.ToBorderStyle(BorderStyle.Default);
        BorderStyle insideV = borders.InsideVerticalBorder.ToBorderStyle(BorderStyle.Default);

        return new CellBorderPattern(top, right, bottom, left, insideH, insideV);
    }

    private static Alignment GetAlignment(this Word.Table table)
    {
        Word.TableProperties tableProps = table.ChildElements.OfType<Word.TableProperties>().Single();
        OpenXml.EnumValue<Word.TableRowAlignmentValues>? ra = tableProps.ChildElements
            .OfType<Word.TableJustification>()
            .SingleOrDefault()
            ?.Val;

        Alignment alignment = ra.GetAlignment();
        return alignment;
    }

    private static Alignment GetAlignment(this OpenXml.EnumValue<Word.TableRowAlignmentValues>? rowAlignmentValue)
    {
        if (rowAlignmentValue is null) return Alignment.Left;
        if (rowAlignmentValue.Value == Word.TableRowAlignmentValues.Right) return Alignment.Right;
        if (rowAlignmentValue.Value == Word.TableRowAlignmentValues.Center) return Alignment.Center;
        return Alignment.Left;
    }
}
