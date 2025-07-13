using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core.Images;
using Proxoft.DocxToPdf.Models.Styles.Services;
using Proxoft.DocxToPdf.Models.Tables.Elements;
using Proxoft.DocxToPdf.Models.Tables.Grids;

namespace Proxoft.DocxToPdf.Models.Tables.Builders;

internal static class CellBuilder
{
    public static Cell[] InitializeCells(
        this Word.Table table,
        IImageAccessor imageAccessor,
        IStyleFactory styleFactory)
    {
        Cell[] cells = [.. table.GetCells(imageAccessor, styleFactory)];
        return cells;
    }

    private static IEnumerable<Cell> GetCells(
        this Word.Table table,
        IImageAccessor imageAccessor,
        IStyleFactory styleFactory)
    {
        Word.TableGrid tg = table.Grid();
        int colCount = tg.Columns().Count();

        Word.TableRow[] rows = [.. table.Rows()];
        if(rows.Length == 0)
        {
            yield break; // No rows to process
        }

        int rowCount = rows.Length;

        Word.TableCell[,] solved = new Word.TableCell[colCount, rowCount];

        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            Word.TableRow row = rows[rowIndex];
            Word.TableCell[] cells = [.. row.Cells()];

            int colIndex = 0;
            for(int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
            {
                Word.TableCell cell = cells[cellIndex];
                if (cell == null || solved[colIndex, rowIndex] != null)
                {
                    colIndex++;
                    continue; // Skip null or already processed cells
                }

                int colSpan = cell.GetColSpan();
                int rowSpan = table.GetVerticalSpan(rowIndex, cellIndex);

                // Create a new Cell instance
                GridPosition gridPosition = new(colIndex, colSpan, rowIndex, rowSpan);
                Cell newCell = Cell.From(cell, gridPosition, imageAccessor, styleFactory);

                // Mark the cells in the grid as solved
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