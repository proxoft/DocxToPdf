using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Tables;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;

namespace Proxoft.DocxToPdf.Builders.Tables;

internal static class CellBuilder
{
    public static IEnumerable<Cell> CreateCells(
        this Word.Table table,
        CellBorderPattern cellBorderPattern,
        BuilderServices services)
    {
        Word.TableGrid tg = table.Grid();
        Word.TableRow[] rows = [.. table.Rows()];

        int colCount = tg.Columns().Count();
        int rowCount = rows.Length;

        if (rows.Length == 0)
        {
            yield break; // No rows to process
        }

        for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            Word.TableRow row = rows[rowIndex];
            Word.TableCell[] cells = [.. row.Cells().Where(c => !c.IsVerticallyMerged())];

            for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
            {
                Word.TableCell cell = cells[cellIndex];

                int colIndex = row.CalculateColumnIndexOfCell(cell);
                int colSpan = cell.GetColSpan();
                int rowSpan = table.GetVerticalSpanOfCell(row, cell);

                GridPosition gridPosition = new(colIndex, colSpan, rowIndex, rowSpan);
                ModelId cellId = services.IdFactory.NextCellId();
                Model[] paragraphsAndTables = cell.CreateParagraphsAndTables(services);
                Borders borders = cell.CreateCellBorders(cellBorderPattern, gridPosition, (colCount, rowCount));

                Cell newCell = new(
                    cellId,
                    gridPosition,
                    paragraphsAndTables,
                    new Padding(4, 0.5f, 4, 0.5f),
                    borders);

                yield return newCell;
            }
        }
    }
}
