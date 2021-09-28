using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf
{
    internal static class TwentiethPoint
    {
        private const double _factor = 20;

        public static double DxaToPoint(this Int32Value value)
            => value.ToDouble(_factor);

        public static double DxaToPoint(this UInt32Value value)
            => value.ToDouble(_factor);

        public static double DxaToPoint(this uint value)
            => value.ToDouble(_factor);

        public static double DxaToPoint(this int value)
            => value.ToDouble(_factor);

        public static double DxaToPoint(this double value)
            => value.ToDouble(_factor);
    }
}
