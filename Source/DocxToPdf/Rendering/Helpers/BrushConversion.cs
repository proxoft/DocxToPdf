﻿using System.Drawing;
using PdfSharp.Drawing;

namespace Proxoft.DocxToPdf.Rendering
{
    internal static class BrushConversion
    {
        public static XColor ToXColor(this Color color)
        {
            var c = XColor.FromArgb(color.ToArgb());
            return c;
        }

        public static XBrush ToXBrush(this Color color)
        {
            return new XSolidBrush(color.ToXColor());
        }
    }
}
