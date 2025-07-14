using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf.Extensions.Units;

internal static class ToFloatConverter
{
    public static float ToFloat(this Int32Value? value, float factor)
    {
        if (value is null || !value.HasValue)
        {
            return 0;
        }

        return value.Value.ToFloat(factor);
    }

    public static float ToFloat(this UInt32Value? value, float factor)
    {
        if (value is null || !value.HasValue)
        {
            return 0;
        }

        return value.Value.ToFloat(factor);
    }

    public static float ToFloat(this uint value, float factor)
    {
        return value / factor;
    }

    public static float ToFloat(this int value, float factor)
    {
        return value / factor;
    }

    public static float ToFloat(this float value, float factor)
    {
        return value / factor;
    }
}
