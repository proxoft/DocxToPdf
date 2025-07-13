using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;

namespace Proxoft.DocxToPdf.Models.Tables.Grids;

internal class Grid(
    double[] columnWidths,
    GridRow[] rowHeights)
{
    private readonly double[] _columnWidths = columnWidths;
    private readonly GridRow[] _gridRows = rowHeights;
    private readonly List<PageContext> _pageContexts = [];

    public Func<PagePosition, PageContext>? PageContextFactory { get; set; }

    public int ColumnCount => _columnWidths.Length;

    public int RowCount => _gridRows.Length;

    public IEnumerable<PageRegion> GetPageRegions()
    {
        var space = this.CalculateHorizontalCellSpace(new GridPosition(0, _columnWidths.Length, 0, 0));
        
        var pageRegions = Enumerable
            .Range(0, _gridRows.Length)
            .SelectMany(i =>
            {
                (PagePosition page, Rectangle region)[] regs = this.FindPageRegionsOfRow(i);
                IEnumerable<PageRegion> pageRegions = regs
                    .Select(pair =>
                    {
                        var rect = new Rectangle(pair.region.TopLeft, new Size(space.Width, pair.region.Height));
                        return new PageRegion(pair.page, rect);
                    });

                return pageRegions;
            })
            .ToArray();

        return pageRegions;
    }

    public void ResetPageContexts(PageContext startOn)
    {
        _pageContexts.Clear();
        _pageContexts.Add(startOn);
    }

    public PageContext CreateStartPageContextForCell(GridPosition position)
    {
        var rowPageContext = this.GetOrCreateRowPageContext(position);
        var horizontalSpace = this.CalculateHorizontalCellSpace(position);
        return rowPageContext.Crop(horizontalSpace);
    }

    public PageContext CreateNextPageContextForCell(PagePosition currentPagePosition, GridPosition position)
    {
        var rowPageContext = this.GetOrCreateNextPageContext(currentPagePosition);
        var horizontalSpace = this.CalculateHorizontalCellSpace(position);
        return rowPageContext.Crop(horizontalSpace);
    }

    public void JustifyGridRows(GridPosition position, IReadOnlyCollection<PageRegion> pageRegions)
    {
        var totalHeightOfCell = pageRegions.Sum(pr => pr.Region.Height);

        if (position.RowSpan == 1)
        {
            _gridRows[position.Row].Expand(totalHeightOfCell);
            return;
        }

        var affectedRows = this.GetRowsInPosition(position)
            .ToArray();

        var rowsSum = affectedRows.Sum(r => r.Height);
        if (rowsSum > totalHeightOfCell)
        {
            return;
        }

        double[] distribution = Distribute([.. affectedRows.Select(r => r.Height)], totalHeightOfCell - rowsSum);
        for (var i = 0; i < distribution.Length; i++)
        {
            affectedRows[i].Expand(distribution[i]);
        }
    }

    public CellBorder GetBorder(GridPosition position)
    {
        HorizontalSpace space = this.CalculateHorizontalCellSpace(position);
        Point lx = new(space.X, 0);
        Point rx = new(space.RightX, 0);

        BorderLine? topLine = null;
        BorderLine? bottomLine = null;
        List<BorderLine> leftLines = [];
        List<BorderLine> rightLines = [];

        for (int i = position.Row; i < position.Row + position.RowSpan; i++)
        {
            (PagePosition page, Rectangle region)[] regions = this.FindPageRegionsOfRow(i);
            if(i == position.Row)
            {
                (PagePosition pagePosition, Rectangle region) = regions.First();
                Point start = region.TopLeft + lx;
                Point end = region.TopLeft + rx;

                topLine = new BorderLine(pagePosition.PageNumber, start, end);
            }

            foreach((PagePosition page, Rectangle region) in regions)
            {
                leftLines.Add(new BorderLine(page.PageNumber, region.TopLeft + lx, region.BottomLeft + lx));
                rightLines.Add(new BorderLine(page.PageNumber, region.TopLeft + rx, region.BottomLeft + rx));
            }

            if(i == position.Row + position.RowSpan - 1)
            {
                (PagePosition pagePosition, Rectangle region) = regions.Last();
                Point start = new(region.X + space.X, region.BottomY);
                Point end = new(region.X + space.RightX, region.BottomY);

                bottomLine = new BorderLine(pagePosition.PageNumber, start, end);
            }
        }

        return new CellBorder(topLine, bottomLine, [..leftLines], [.. rightLines]);
    }

    private HorizontalSpace CalculateHorizontalCellSpace(GridPosition position)
    {
        var offset = _columnWidths
           .Take(position.Column)
           .Aggregate(0d, (col, acc) => acc + col);

        var width = _columnWidths
          .Skip(position.Column)
          .Take(position.ColumnSpan)
          .Aggregate(0.0, (col, acc) => acc + col);

        return new HorizontalSpace(offset, width);
    }

    private double RowAbsoluteYOffset(GridPosition position) =>
        this.RowAbsoluteYOffset(position.Row);

    private double RowAbsoluteYOffset(int rowIndex) =>
        _gridRows
            .Take(rowIndex)
            .Sum(gr => gr.Height);

    private static double[] Distribute(double[] currentValues, double totalValueToDistribute)
    {
        if (totalValueToDistribute <= 0)
        {
            return currentValues;
        }

        double perItem = totalValueToDistribute / currentValues.Length;
        double[] copy = [
            ..currentValues
                .Select((v, i) =>
                {
                    var newValue = i < currentValues.Length - 1
                        ? v + perItem
                        : v + (totalValueToDistribute - (i * perItem));

                    return newValue;
                })
        ];

        return copy;
    }

    private IEnumerable<GridRow> GetRowsInPosition(GridPosition position) =>
        _gridRows
            .Skip(position.Row)
            .Take(position.RowSpan);

    private PageContext GetOrCreateRowPageContext(GridPosition position)
    {
        double rowOffset = this.RowAbsoluteYOffset(position);
        PageContext pageContext = _pageContexts.First();
        do
        {
            if (pageContext.Region.Height > rowOffset)
            {
                return pageContext.Crop(rowOffset, 0, 0, 0);
            }

            rowOffset -= pageContext.Region.Height;
            pageContext = this.GetOrCreateNextPageContext(pageContext.PagePosition);
        } while (true);
    }

    private PageContext GetOrCreateNextPageContext(PagePosition currentPagePosition)
    {
        PageContext? nextPageContext = _pageContexts.FirstOrDefault(pc => pc.PagePosition == currentPagePosition.Next());
        if(nextPageContext != null)
        {
            return nextPageContext;
        }

        if(this.PageContextFactory is null)
        {
            throw new Exception("PageContext factory is null");
        }

        nextPageContext = this.PageContextFactory(currentPagePosition);
        _pageContexts.Add(nextPageContext);
        return nextPageContext;
    }

    private (PagePosition page, Rectangle region)[] FindPageRegionsOfRow(int rowIndex)
    {
        var offset = this.RowAbsoluteYOffset(rowIndex);
        var remainingHeight = _gridRows[rowIndex].Height;

        List<(PagePosition, Rectangle)> regions = [];

        foreach (var pg in _pageContexts)
        {
            if (pg.Region.Height < offset)
            {
                offset -= pg.Region.Height;
                continue;
            }

            var availableHeight = pg.Region.Height - offset;

            var h = Math.Min(availableHeight, remainingHeight);
            var region = pg.Region.Crop(offset, 0, pg.Region.Height - offset - h, 0);
            regions.Add((pg.PagePosition, region));
            offset = 0;
            remainingHeight -= h;
            if (remainingHeight == 0)
            {
                break;
            }
        }

        return [.. regions];
    }
}
