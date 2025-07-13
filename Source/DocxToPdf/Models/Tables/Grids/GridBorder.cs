using System.Collections.Generic;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Tables.Elements;

using Drawing = System.Drawing;

namespace Proxoft.DocxToPdf.Models.Tables.Grids;

internal class GridBorder(
    TableBorderStyle tableBorderStyle,
    Grid grid)
{
    private readonly TableBorderStyle _tableBorderStyle = tableBorderStyle;
    private readonly Grid _grid = grid;

    public void Render(IEnumerable<Cell> cells, Point pageOffset, IRenderer renderer)
    {
        foreach(var cell in cells)
        {
            var border = _grid.GetBorder(cell.GridPosition);
            this.RenderBorders(renderer, cell.GridPosition, cell.BorderStyle, border, pageOffset);
        }
    }

    private void RenderBorders(
        IRenderer renderer,
        GridPosition gridPosition,
        BorderStyle borderStyle,
        CellBorder borders,
        Point pageOffset)
    {
        Drawing.Pen? topPen = this.TopPen(borderStyle, gridPosition);
        RenderBorderLine(renderer, borders.Top, topPen, pageOffset);

        Drawing.Pen? bottomPen = this.BottomPen(borderStyle, gridPosition);
        RenderBorderLine(renderer, borders.Bottom, bottomPen, pageOffset);

        Drawing.Pen? leftPen = this.LeftPen(borderStyle, gridPosition);
        foreach(var lb in borders.Left)
        {
            RenderBorderLine(renderer, lb, leftPen, pageOffset);
        }

        Drawing.Pen? rightPen = this.RightPen(borderStyle, gridPosition);
        foreach (var rb in borders.Right)
        {
            RenderBorderLine(renderer, rb, rightPen, pageOffset);
        }
    }

    private static void RenderBorderLine(
        IRenderer renderer,
        BorderLine? borderLine,
        Drawing.Pen? pen,
        Point pageOffset)
    {
        if(borderLine is null)
        {
            return;
        }

        IRendererPage page = renderer.GetPage(borderLine.PageNumber).Offset(pageOffset);
        Line line = borderLine.ToLine(pen);
        page.RenderLine(line);
    }

    private Drawing.Pen? TopPen(BorderStyle border, GridPosition position)
        => border.Top ?? this.DefaultTopPen(position);

    private Drawing.Pen? LeftPen(BorderStyle border, GridPosition position)
        => border.Left ?? this.DefaultLeftPen(position);

    private Drawing.Pen? RightPen(BorderStyle border, GridPosition position)
        => border.Right ?? this.DefaultRightPen(position);

    private Drawing.Pen? BottomPen(BorderStyle border, GridPosition position)
        => border.Bottom ?? this.DefaultBottomPen(position);

    private Drawing.Pen? DefaultTopPen(GridPosition position)
    {
        return position.Row == 0
            ? _tableBorderStyle.Top
            : _tableBorderStyle.InsideHorizontal;
    }

    private Drawing.Pen? DefaultLeftPen(GridPosition position)
    {
        return position.Column == 0
            ? _tableBorderStyle.Left
            : _tableBorderStyle.InsideVertical;
    }

    private Drawing.Pen? DefaultRightPen(GridPosition position)
    {
        return position.Column + position.ColumnSpan == _grid.ColumnCount
            ? _tableBorderStyle.Right
            : _tableBorderStyle.InsideVertical;
    }

    private Drawing.Pen? DefaultBottomPen(GridPosition position)
    {
        return position.Row + position.RowSpan == _grid.RowCount
            ? _tableBorderStyle.Bottom
            : _tableBorderStyle.InsideHorizontal;
    }
}
