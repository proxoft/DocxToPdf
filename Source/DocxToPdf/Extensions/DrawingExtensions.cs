using System;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Extensions.Units;

namespace Proxoft.DocxToPdf.Extensions;

internal static class DrawingExtensions
{
    public static Size ToSize(this Extent? extent)
    {
        double width = extent?.Cx.EmuToPoint() ?? 0;
        double height = extent?.Cy.EmuToPoint() ?? 0;
        return new Size(width, height);
    }

    public static double ToDouble(this PositionOffset? positionOffset)
    {
        if (positionOffset is null)
        {
            return 0;
        }

        var offset = Convert.ToInt64(positionOffset.Text);
        return offset.EmuToPoint();
    }
}
