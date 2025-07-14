using Proxoft.DocxToPdf.Extensions.Conversions;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Tables.Elements;

using SDraw = System.Drawing;

namespace Proxoft.DocxToPdf.Models.Tables.Builders;

internal static class BorderStyleBuilder
{
    public static TableBorderStyle GetBorder(this Word.TableBorders? borders)
    {
        if (borders is null)
        {
            return TableBorderStyle.Default;
        }

        SDraw.Pen? top = borders.TopBorder.ToPen();
        SDraw.Pen? right = borders.RightBorder.ToPen();
        SDraw.Pen? bottom = borders.BottomBorder.ToPen();
        SDraw.Pen? left = borders.LeftBorder.ToPen();
        SDraw.Pen? insideH = borders.InsideHorizontalBorder.ToPen(TableBorderStyle.Default.InsideHorizontal);
        SDraw.Pen? insideV = borders.InsideVerticalBorder.ToPen(TableBorderStyle.Default.InsideVertical);

        return new TableBorderStyle(top, right, bottom, left, insideH, insideV);
    }

    public static BorderStyle GetBorderStyle(this Word.TableCell cell) =>
        cell.TableCellProperties.GetBorderStyle();

    private static BorderStyle GetBorderStyle(this Word.TableCellProperties? properties) =>
        properties?.TableCellBorders.ToCellBorderStyle() ?? BorderStyle.NoBorder;

    private static BorderStyle? ToCellBorderStyle(this Word.TableCellBorders? borders)
    {
        if (borders == null)
        {
            return null;
        }

        SDraw.Pen? top = borders.TopBorder.ToPen();
        SDraw.Pen? right = borders.RightBorder.ToPen();
        SDraw.Pen? bottom = borders.BottomBorder.ToPen();
        SDraw.Pen? left = borders.LeftBorder.ToPen();

        return new BorderStyle(top, right, bottom, left);
    }
}
