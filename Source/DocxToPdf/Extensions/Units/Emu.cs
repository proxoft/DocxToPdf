using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf.Extensions.Units;

internal static class Emu
{
    private const double _inch = 72;
    private const double _emu = 914400;

    public static double EmuToPoint(this UInt32Value? value)
    {
        if(value?.Value is null)
        {
            return 0;
        }

        var v = System.Convert.ToInt64(value.Value);
        return v.EmuToPoint();
    }

    public static double EmuToPoint(this Int64Value? value)
    {
        if(value?.Value is null)
        {
            return 0;
        }

        return value.Value.EmuToPoint();
    }

    public static double EmuToPoint(this long value)
    {
        return value / _emu * _inch;
    }
}
