using Proxoft.DocxToPdf.Core.Structs;

namespace Proxoft.DocxToPdf.Models.Common;

internal class DocumentPosition
{
    public static readonly DocumentPosition None = new DocumentPosition(PagePosition.None, Point.Zero);

    public DocumentPosition(PagePosition pagePosition, Point offset)
    {
        this.Page = pagePosition;
        this.Offset = offset;
    }

    public Point Offset { get; }
    public PagePosition Page { get; }

    public DocumentPosition Move(Point offset)
        => this + offset;

    public DocumentPosition MoveX(double xOffset)
        => this + new Point(xOffset, 0);

    public DocumentPosition MoveY(double yOffset)
        => this + new Point(0, yOffset);

    public static DocumentPosition operator+(DocumentPosition position, Point offset)
    {
        return new DocumentPosition(position.Page, position.Offset + offset);
    }
}
