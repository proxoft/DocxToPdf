using System.Drawing;
using Proxoft.DocxToPdf.Extensions.Units;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Tables.Elements;

internal class TableBorderStyle(Pen? top, Pen? right, Pen? bottom, Pen? left, Pen? insideHorizontal, Pen? insideVertical) : BorderStyle(top, right, bottom, left)
{
    private static readonly Pen _defaultPen = new(Color.Black, 4.EpToPoint());

    public static readonly TableBorderStyle Default = new(_defaultPen, _defaultPen, _defaultPen, _defaultPen, _defaultPen, _defaultPen);

    public Pen? InsideHorizontal { get; } = insideHorizontal;

    public Pen? InsideVertical { get; } = insideVertical;
}
