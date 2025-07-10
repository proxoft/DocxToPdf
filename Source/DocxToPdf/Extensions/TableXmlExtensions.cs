using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf;

internal static class TableXmlExtensions
{
    public static TableProperties Properties(this Table table)
    {
        return table.ChildElements.OfType<TableProperties>().Single();
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

    public static TableGrid Grid(this Table table)
    {
        return table.ChildElements.OfType<TableGrid>().Single();
    }

    public static IEnumerable<GridColumn> Columns(this TableGrid grid)
    {
        return grid.ChildElements.OfType<GridColumn>();
    }

    public static int GetColSpan(this TableCell cell)
    {
        var span = cell.GetFirstChild<TableCellProperties>()?.GetFirstChild<GridSpan>()?.Val?.Value;
        return span.HasValue ? span.Value : 1;
    }

    public static int GetVerticalSpan(this Table table, int rowIndex, int cellIndex)
    {
        var rows = table.Elements<TableRow>().ToList();

        // Get the starting cell
        var startCell = rows[rowIndex].Elements<TableCell>().ElementAt(cellIndex);

        var vMerge = startCell.GetFirstChild<TableCellProperties>()?.GetFirstChild<VerticalMerge>();
        if (vMerge == null || vMerge.Val?.Value != MergedCellValues.Restart)
            return 1; // Not the start of a vertical merge

        int span = 1;

        for (int i = rowIndex + 1; i < rows.Count; i++)
        {
            var cells = rows[i].Elements<TableCell>().ToList();
            if (cellIndex >= cells.Count)
                break;

            var cell = cells[cellIndex];
            var props = cell.GetFirstChild<TableCellProperties>();
            var merge = props?.GetFirstChild<VerticalMerge>();

            if (merge != null && (merge.Val == null || merge.Val.Value == MergedCellValues.Continue))
            {
                span++;
            }
            else
            {
                break;
            }
        }

        return span;
    }
}
