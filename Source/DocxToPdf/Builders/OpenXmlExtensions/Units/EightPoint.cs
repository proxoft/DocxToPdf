using System;
using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;

internal static class EightPoint
{
    private const float _factor = 8;

    public static float EpToPoint(this StringValue? value, float ifNull = 0)
    {
        if (value?.Value is null)
        {
            return ifNull;
        }

        int v = Convert.ToInt32(value.Value);
        return v.EpToPoint();
    }

    public static float EpToPoint(this UInt32Value? value, float ifNull = 0)
    {
        if (value?.Value is null)
        {
            return ifNull;
        }

        int v = Convert.ToInt32(value.Value);
        return v.EpToPoint();
    }

    public static float EpToPoint(this int value)
        => value.ToFloat(_factor);
}
