using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Structs;

namespace Proxoft.DocxToPdf.Rendering.Helpers;

internal static class GeometriesConversions
{
    public static XPoint ToXPoint(this Point point) =>
        new (point.X, point.Y);

    public static XVector ToXVector(this Point point) =>
        new (point.X, point.Y);

    public static XRect ToXRect(this Rectangle rectangle) =>
        new (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

    public static XRect ToXRect(this Rectangle rectangle, XVector offset)
    {
        XRect rect = new (rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        return XRect.Offset(rect, offset);
    }

    public static XSize ToXSize(this Size size) =>
        new (size.Width, size.Height);

    public static Size ToSize(this XSize size) =>
        new(size.Width, size.Height);
}
