using DocumentFormat.OpenXml;
using Proxoft.DocxToPdf.Extensions.Units;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;

internal static class TwentiethPoint
{
    private const float _factor = 20;

    public static float DxaToPoint(this Int32Value? value)
        => value.ToFloat(_factor);

    public static float DxaToPoint(this UInt32Value? value)
        => value.ToFloat(_factor);

    public static float DxaToPoint(this uint value)
        => value.ToFloat(_factor);

    public static float DxaToPoint(this int value)
        => value.ToFloat(_factor);

    public static float DxaToPoint(this float value)
        => value.ToFloat(_factor);
}
