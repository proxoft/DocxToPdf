using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Extensions.Units;

namespace Proxoft.DocxToPdf.Builders.Tables;

internal static class BordersBuilder
{
    public static Borders CreateCellBorders(
        this Word.TableCell cell,
        CellBorderPattern cellBorderPattern,
        GridPosition gridPosition,
        (int colCount, int rowCount) gridSize) =>
        cell.TableCellProperties?.TableCellBorders.CreateBorders(cellBorderPattern, gridPosition, gridSize)
            ?? cellBorderPattern.CreateBorders(gridPosition, gridSize);

    private static Borders? CreateBorders(
        this Word.TableCellBorders? borders,
        CellBorderPattern cellBorderPattern,
        GridPosition gridPosition,
        (int colCount, int rowCount) gridSize
    )
    {
        if (borders is null)
        {
            return null;
        }

        BorderStyle left = borders.LeftBorder.ToBorderStyle(cellBorderPattern.CreateLeftBorderStyle(gridPosition));
        BorderStyle top = borders.TopBorder.ToBorderStyle(cellBorderPattern.CreateTopBorderStyle(gridPosition));
        BorderStyle right = borders.RightBorder.ToBorderStyle(cellBorderPattern.CreateRightBorderStyle(gridPosition, gridSize.colCount));
        BorderStyle bottom = borders.BottomBorder.ToBorderStyle(cellBorderPattern.CreateBottomBorderStyle(gridPosition, gridSize.rowCount));

        return new Borders(left, top, right, bottom);
    }

    private static Borders CreateBorders(this CellBorderPattern cellBorderPattern, GridPosition gridPosition, (int colCount, int rowCount) gridSize)
    {
        BorderStyle left = cellBorderPattern.CreateLeftBorderStyle(gridPosition);
        BorderStyle top = cellBorderPattern.CreateTopBorderStyle(gridPosition);
        BorderStyle right = cellBorderPattern.CreateRightBorderStyle(gridPosition, gridSize.colCount);
        BorderStyle bottom = cellBorderPattern.CreateBottomBorderStyle(gridPosition, gridSize.rowCount);

        return new Borders(left, top, right, bottom);
    }

    private static BorderStyle CreateLeftBorderStyle(this CellBorderPattern cellBorderPattern, GridPosition gridPosition) =>
        gridPosition.Column == 0
            ? cellBorderPattern.Left
            : cellBorderPattern.Vertical;

    private static BorderStyle CreateTopBorderStyle(this CellBorderPattern cellBorderPattern, GridPosition gridPosition) =>
        gridPosition.Row == 0
            ? cellBorderPattern.Top
            : cellBorderPattern.Horizontal;

    private static BorderStyle CreateRightBorderStyle(this CellBorderPattern cellBorderPattern, GridPosition gridPosition, int colCount) =>
        (gridPosition.Column + gridPosition.ColumnSpan) == colCount
            ? cellBorderPattern.Right
            : cellBorderPattern.Vertical;

    private static BorderStyle CreateBottomBorderStyle(this CellBorderPattern cellBorderPattern, GridPosition gridPosition, int rowCount) =>
        (gridPosition.Row + gridPosition.RowSpan) == rowCount
            ? cellBorderPattern.Bottom
            : cellBorderPattern.Horizontal;

    public static BorderStyle ToBorderStyle(this Word.BorderType? border) =>
        border.ToBorderStyle(BorderStyle.None);

    public static BorderStyle ToBorderStyle(this Word.BorderType? border, BorderStyle ifNull)
    {
        if (border is null)
        {
            return ifNull;
        }

        Color color = border.Color.ToColor();
        float width = border.Size.EpToPoint();
        LineStyle lineStyle = border.Val.ToLineStyle();

        return new BorderStyle(color, width, lineStyle);
    }

    private static Color ToColor(this OpenXml.StringValue? color) =>
        new(color?.Value ?? "auto");
}
