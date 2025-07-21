using System.Drawing;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Extensions.Conversions;
using Proxoft.DocxToPdf.Extensions.Units;

namespace Proxoft.DocxToPdf.Builders.Tables;

internal static class BordersBuilder
{
    public static Borders CreateCellBorders(this Word.TableCell cell) =>
        cell.TableCellProperties?.TableCellBorders.CreateBorders() ?? Borders.None;

    private static Borders CreateBorders(this Word.TableCellBorders? borders)
    {
        if (borders == null)
        {
            return Borders.None;
        }

        BorderStyle left = borders.LeftBorder.ToBorderStyle();
        BorderStyle top = borders.TopBorder.ToBorderStyle();
        BorderStyle right = borders.RightBorder.ToBorderStyle();
        BorderStyle bottom = borders.BottomBorder.ToBorderStyle();

        return new Borders(left, top, right, bottom);
    }

    public static BorderStyle ToBorderStyle(this Word.BorderType? border)
    {
        if (border is null)
        {
            return BorderStyle.None;
        }

        string hexColor = border.Color.ToHexColor();
        float width = border.Size.EpToPoint();
        // var val = border.Val?.Value ?? Word.BorderValues.Single;
        // pen.UpdateStyle(val);
        return new BorderStyle(new Documents.Styles.Color(hexColor), width, LineStyle.Solid);
    }

    private static string ToHexColor(this OpenXml.StringValue? color)
    {
        string? value = color?.Value;
        if(value is null || value == "auto")
        {
            return "000000";
        }

        return value;
    }
}
