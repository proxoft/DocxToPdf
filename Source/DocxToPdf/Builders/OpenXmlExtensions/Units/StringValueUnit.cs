using System;
using System.Linq;
using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;

internal static class StringValueUnit
{
    
    private static readonly string[] _units = ["mm", "cm", "in", "pt", "pc", "pi"];

    public static float ToPoint(this StringValue? value, float ifNull = 0)
    {
        if (value is null)
        {
            return ifNull;
        }

        (float v, string u) = value.ToValueWithUnit();
        return u switch
        {
            "mm" => v.PointsFromMilimeter(), //(double)XUnit.FromMillimeter(v),
            "cm" => v.PointsFromCentimeter(),
            "in" => 0,// v.InchToPoint();
            "pt" => v.DxaToPoint(),
            "pi" => v.PointsFromPresentationPoints(),
            _ => throw new Exception($"Unhandled string value: {value}"),
        };
    }

    public static long ToLong(this StringValue value)
    {
        return Convert.ToInt64(value.Value);
    }

    private static (float v, string unit) ToValueWithUnit(this StringValue? stringValue, float ifNull = 0)
    {
        if (stringValue?.Value is null)
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
        return (Convert.ToSingle(v), u);
    }
}
