using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Tables;

internal static class TableExtensions
{
    public static TableGrid Grid(this Table table)
    {
        return table.ChildElements.OfType<TableGrid>().Single();
    }

    public static IEnumerable<GridColumn> Columns(this TableGrid grid)
    {
        return grid.ChildElements.OfType<GridColumn>();
    }

    public static IEnumerable<TableRow> Rows(this Table table)
    {
        return table.ChildElements.OfType<TableRow>();
    }

    public static IEnumerable<TableCell> Cells(this TableRow row)
    {
        return row.ChildElements
            .Where(c => c is TableCell || c is SdtCell)
            .Select(c =>
            {
                return c switch
                {
                    TableCell tc => tc,
                    SdtCell sdt when sdt.SdtContentCell != null => sdt.SdtContentCell.ChildElements.OfType<TableCell>().First(),
                    _ => throw new RendererException($"Unexpected element {c.GetType().Name} in table row")
                };
            })
            .Cast<TableCell>();
    }

    public static int GetColSpan(this TableCell cell)
    {
        int? span = cell.GetFirstChild<TableCellProperties>()?.GetFirstChild<GridSpan>()?.Val?.Value;
        return span ?? 1;
    }

    public static int GetVerticalSpanOfCell(this Word.Table table, Word.TableRow row, Word.TableCell cell)
    {
        Word.TableRow[] nextRows = [.. table.Rows().SkipWhile(r => r != row).Skip(1)];
        int colIndexOfCell = row.CalculateColumnIndexOfCell(cell);
        int rowSpan = 1;
        for(int ri = 0; ri < nextRows.Length; ri++)
        {
            Word.TableCell nextCell = nextRows[ri].FindCellAtColumnIndex(colIndexOfCell);
            if (nextCell.IsVerticallyMerged())
            {
                rowSpan++;
            }
            else
            {
                break;
            }
        }

        return rowSpan;
    }

    public static int CalculateColumnIndexOfCell(this Word.TableRow row, Word.TableCell cell)
    {
        int colCounts = row
            .Cells()
            .TakeWhile(c => c != cell)
            .Select(c => c.GetColSpan())
            .Sum();

        return colCounts;
    }

    private static Word.TableCell FindCellAtColumnIndex(this Word.TableRow row, int columnIndex)
    {
        int colIndex = -1;
        Word.TableCell[] cells = [.. row.Cells()];
        foreach (Word.TableCell cell in cells)
        {
            int colSpan = cell.GetColSpan();
            colIndex += colSpan;
            if (colIndex >= columnIndex)
            {
                return cell;
            }
        }

        throw new System.Exception($"Cell at {columnIndex} not found");
    }

    public static bool IsVerticallyMerged(this Word.TableCell cell)
    {
        Word.VerticalMerge? vMerge = cell.GetFirstChild<Word.TableCellProperties>()?.GetFirstChild<Word.VerticalMerge>();
        return (vMerge != null && vMerge.Val?.Value != Word.MergedCellValues.Restart);
    }
}
