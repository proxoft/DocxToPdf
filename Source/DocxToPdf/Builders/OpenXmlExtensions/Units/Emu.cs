using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;

internal static class Emu
{
    private const float _inch = 72;
    private const float _emu = 914400;

    public static float EmuToPoint(this UInt32Value? value)
    {
        if(value?.Value is null)
        {
            return 0;
        }

        long v = System.Convert.ToInt64(value.Value);
        return v.EmuToPoint();
    }

    public static float EmuToPoint(this Int64Value? value)
    {
        if(value?.Value is null)
        {
            return 0;
        }

        return value.Value.EmuToPoint();
    }

    public static float EmuToPoint(this long value)
    {
        return value / _emu * _inch;
    }
}
