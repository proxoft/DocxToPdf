using System;
using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;

internal static class HalfPoint
{
    private const float _factor = 2;

    public static float HPToPoint(this StringValue value, float ifNull)
    {
        if (value?.Value == null)
        {
            return ifNull;
        }

        int v = Convert.ToInt32(value.Value);
        return v.HPToPoint();
    }

    public static float HPToPoint(this int value) =>
        value.ToFloat(_factor);
}
