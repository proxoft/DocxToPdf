using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Extensions;

namespace Proxoft.DocxToPdf.Builders.Tables;

internal static class CellBuilder
{
    public static IEnumerable<Cell> CreateCells(this Word.Table table, BuilderServices services)
    {
        Word.TableGrid tg = table.Grid();
        Word.TableRow[] rows = [.. table.Rows()];
        if (rows.Length == 0)
        {
            yield break; // No rows to process
        }

        int colCount = tg.Columns().Count();
        int rowCount = rows.Length;

        Word.TableCell[,] solved = new Word.TableCell[colCount, rowCount];

        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            Word.TableRow row = rows[rowIndex];
            Word.TableCell[] cells = [.. row.Cells()];

            int colIndex = 0;
            for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
            {
                Word.TableCell? cell = cellIndex < cells.Length
                    ? cells[cellIndex]
                    : null;

                if(cell is null || solved[colIndex, rowIndex] != null)
                {
                    colIndex++;
                    continue; // Skip null or already processed cells
                }

                int colSpan = cell.GetColSpan();
                int rowSpan = table.GetVerticalSpan(rowIndex, cellIndex);
                GridPosition gridPosition = new(colIndex, colSpan, rowIndex, rowSpan);
                ModelId cellId = services.IdFactory.NextCellId();
                Model[] paragraphsAndTables = cell.CreateParagraphsAndTables(services);
                Borders borders = cell.CreateCellBorders();

                Cell newCell = new(
                    cellId,
                    gridPosition,
                    paragraphsAndTables,
                    new Padding(0.5f, 4, 0.5f, 4),
                    borders);

                for (int r = rowIndex; r < rowIndex + rowSpan; r++)
                {
                    for (int c = colIndex; c <= colIndex + (colSpan - 1); c++)
                    {
                        solved[c, r] = cell;
                    }
                }

                colIndex += colSpan;
                yield return newCell;
            }
        }
    }
}
