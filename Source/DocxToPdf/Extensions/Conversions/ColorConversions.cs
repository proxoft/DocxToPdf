﻿using System.Drawing;
using System.Globalization;
using OpenXml = DocumentFormat.OpenXml;
using Word = DocumentFormat.OpenXml.Wordprocessing;

using static DocumentFormat.OpenXml.Wordprocessing.HighlightColorValues;

namespace Proxoft.DocxToPdf
{
    internal static class ColorConversions
    {
        public static Color ToColor(this Word.Highlight? highlight)
        {
            var colorName = highlight?.Val ?? None;
            var c = colorName.Value.ToColor();
            return c;
        }

        public static Color ToColor(this Word.Color? color)
        {
            if(color is null)
            {
                return Color.Black;
            }

            return color.Val.ToColor();
        }

        public static Color ToColor(this OpenXml.StringValue? color)
        {
            var hex = color?.Value;
            var result = hex.ToColor();
            return result;
        }

        private static Color ToColor(this string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex) || hex == "auto")
            {
                return Color.FromArgb(0, 0, 0);
            }

            var (r, g, b) = hex.ToRgb();
            return Color.FromArgb(r, g, b);
        }

        private static (int r, int g, int b) ToRgb(this string hex)
        {
            var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return (r, g, b);
        }

        private static Color ToColor(this Word.HighlightColorValues name)
        {
            return name switch
            {
                Black => Color.FromArgb(0, 0, 0),
                Blue => Color.FromArgb(0, 0, 0xFF),
                Cyan => Color.FromArgb(0, 0xFF, 0xFF),
                Green => Color.FromArgb(0, 0xFF, 0),
                Magenta => Color.FromArgb(0xFF, 0, 0xFF),
                Red => Color.FromArgb(0xFF, 0, 0),
                Yellow => Color.FromArgb(0xFF, 0xFF, 0),
                White => Color.FromArgb(0xFF, 0xFF, 0xFF),
                DarkBlue => Color.FromArgb(0, 0, 0x80),
                DarkCyan => Color.FromArgb(0, 0x80, 0x80),
                DarkGreen => Color.FromArgb(0, 0x80, 0),
                DarkMagenta => Color.FromArgb(0x80, 0, 0x80),
                DarkRed => Color.FromArgb(0x80, 0, 0),
                DarkYellow => Color.FromArgb(0x80, 0x80, 0),
                DarkGray => Color.FromArgb(0x80, 0x80, 0x80),
                LightGray => Color.FromArgb(0xC0, 0xC0, 0xC0),
                None => Color.Empty,
                _ => Color.Empty
            };
        }
    }
}
