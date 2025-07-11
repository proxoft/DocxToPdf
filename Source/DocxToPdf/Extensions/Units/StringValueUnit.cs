using System;
using System.Linq;
using DocumentFormat.OpenXml;
using PdfSharp.Drawing;

namespace Proxoft.DocxToPdf.Extensions.Units;

internal static class StringValueUnit
{
    private static readonly string[] _units = [ "mm", "cm", "in", "pt", "pc", "pi" ];

    public static double ToPoint(this StringValue? value, double ifNull = 0)
    {
        if (value is null)
        {
            return ifNull;
        }

        var (v, u) = value.ToValueWithUnit();
        return u switch
        {
            "mm" => (double)XUnit.FromMillimeter(v),
            "cm" => (double)XUnit.FromCentimeter(v),
            "in" => 0,// v.InchToPoint();
            "pt" => v.DxaToPoint(),
            "pi" => (double)XUnit.FromPresentation(v),
            _ => throw new Exception($"Unhandled string value: {value}"),
        };
    }

    public static long ToLong(this StringValue value)
    {
        return Convert.ToInt64(value.Value);
    }

    private static (double v, string unit) ToValueWithUnit(this StringValue? stringValue, double ifNull = 0)
    {
        if(stringValue?.Value is null)
        {
            return (ifNull, "pt");
        }

        int l = stringValue.Value.Length > 2
            ? stringValue.Value.Length - 2
            : 0;

        string u = stringValue.Value[l..];

        if (!_units.Contains(u))
        {
            l = stringValue.Value.Length;
            u = "pt";
        }

        string v = stringValue.Value[..l];
        return (Convert.ToDouble(v), u);
    }
}
