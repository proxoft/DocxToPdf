using System.Linq;
using Proxoft.DocxToPdf.Models.Styles.Services;
using Proxoft.DocxToPdf.Models.Tables.Grids;
using Proxoft.DocxToPdf.Models.Tables.Elements;
using Proxoft.DocxToPdf.Core.Images;

namespace Proxoft.DocxToPdf.Models.Tables.Builders;

internal static class TableFactory
{
    public static Table Create(Word.Table wordTable, IImageAccessor imageAccessor, IStyleFactory styleFactory)
    {
        Grid grid = wordTable.InitializeGrid();

        Cell[] cells = [
            ..wordTable
                .InitializeCells(imageAccessor, styleFactory.ForTable(wordTable.Properties()))
                .OrderBy(c => c.GridPosition.Row)
                .ThenBy(c => c.GridPosition.Column)
        ];

        TableBorderStyle tableBorder = wordTable
            .Properties()
            .TableBorders
            .GetBorder();

        return new Table(cells, grid, tableBorder);
    }
}
