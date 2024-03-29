﻿using System;
using DocumentFormat.OpenXml;

namespace Proxoft.DocxToPdf
{
    internal static class HalfPoint
    {
        private const double _factor = 2;

        public static double HPToPoint(this StringValue value, double ifNull)
        {
            if(value?.Value == null)
            {
                return ifNull;
            }

            var v = Convert.ToInt32(value.Value);
            return v.HPToPoint();
        }

        public static double HPToPoint(this int value)
            => value.ToDouble(_factor);
    }
}
