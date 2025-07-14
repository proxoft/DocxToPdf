using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Tables.Elements;
using Proxoft.DocxToPdf.Models.Tables.Grids;

namespace Proxoft.DocxToPdf.Models.Tables;

internal class Table(IEnumerable<Cell> cells, Grid grid, TableBorderStyle tableBorder) : PageContextElement
{
    private readonly Cell[] _cells = [.. cells];
    private readonly Grid _grid = grid;
    private readonly TableBorderStyle _tableBorder = tableBorder;

    private Point _pageOffset = Point.Zero;

    private IEnumerable<Cell> PreparationOrderedCells => _cells
        .OrderBy(c => c.GridPosition.RowSpan) // start with smalles cells
        .ThenBy(c => c.GridPosition.Row)      // from top
        .ThenBy(c => c.GridPosition.Column);  // from left

    public override void Prepare(PageContext pageContext, Func<PagePosition, PageContextElement, PageContext> nextPageContextFactory)
    {
        _grid.ResetPageContexts(pageContext);
        _grid.PageContextFactory = currentPagePosition => nextPageContextFactory(currentPagePosition, this);

        PageContext onNextPageContext(PagePosition currentPagePosition, PageContextElement childElement)
            => _grid.CreateNextPageContextForCell(currentPagePosition, ((Cell)childElement).GridPosition);

        Rectangle availableRegion = pageContext.Region;
        foreach (Cell cell in this.PreparationOrderedCells)
        {
            PageContext cellPageContext = _grid.CreateStartPageContextForCell(cell.GridPosition);
            cell.Prepare(cellPageContext, onNextPageContext);

            _grid.JustifyGridRows(cell.GridPosition, cell.PageRegions);
        }

        _grid.PageContextFactory = null;
        this.ResetPageRegions(_grid.GetPageRegions());
    }

    public override void Render(IRenderer renderer)
    {
        _cells.Render(renderer);
        new GridBorder(_tableBorder, _grid).Render(_cells, _pageOffset, renderer);
    }

    public override void SetPageOffset(Point pageOffset)
    {
        _pageOffset = pageOffset;
        foreach (Cell cell in _cells)
        {
            cell.SetPageOffset(pageOffset);
        }
    }
}
