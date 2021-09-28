using System;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf
{
    internal static class DrawingExtensions
    {
        public static Size ToSize(this Extent extent)
        {
            var width = extent.Cx.EmuToPoint();
            var height = extent.Cy.EmuToPoint();
            return new Size(width, height);
        }

        public static double ToPoint(this PositionOffset positionOffset)
        {
            var offset = Convert.ToInt64(positionOffset.Text);
            return offset.EmuToPoint();
        }
    }
}
