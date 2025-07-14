using Proxoft.DocxToPdf.Core.Structs;

namespace Proxoft.DocxToPdf.Models.Common;

internal class DocumentPosition(PagePosition pagePosition, Point offset)
{
    public static readonly DocumentPosition None = new(PagePosition.None, Point.Zero);

    public Point Offset { get; } = offset;

    public PagePosition Page { get; } = pagePosition;

    public DocumentPosition Move(Point offset) =>
        this + offset;

    public DocumentPosition MoveX(double xOffset)=>
        this + new Point(xOffset, 0);

    public DocumentPosition MoveY(double yOffset) =>
        this + new Point(0, yOffset);

    public static DocumentPosition operator+(DocumentPosition position, Point offset) =>
        new(position.Page, position.Offset + offset);
}
