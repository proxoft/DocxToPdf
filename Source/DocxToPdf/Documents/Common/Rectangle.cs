using System.Collections.Generic;
using System.Linq;

namespace Proxoft.DocxToPdf.Documents.Common;

internal record Rectangle(float X, float Y, float Width, float Height)
{
    public static readonly Rectangle Empty = new(0,0,0,0);

    public Rectangle(Position position, Size size) : this(position.X, position.Y, size.Width, size.Height)
    {
    }

    public float Right => this.X + this.Width;

    public float Bottom => this.Y + this.Height;

    public Position TopLeft => new(this.X, this.Y);

    public Position BottomRight => new(this.X + this.Width, this.Y + this.Width);

    public Size Size => new(this.Width, this.Height);

    public Rectangle CropFromLeft(float delta) =>
        FromCorners(new Position(this.X + delta, this.Y), this.BottomRight);

    public Rectangle CropFromTop(float delta) =>
        FromCorners(new Position(this.X, this.Y + delta), this.BottomRight);

    public Rectangle CropFromRight(float delta) =>
        FromCorners(this.TopLeft, new Position(this.BottomRight.X - delta, this.BottomRight.Y));

    public Rectangle CropFromBottom(float delta) =>
        FromCorners(this.TopLeft, new Position(this.BottomRight.X, this.BottomRight.Y - delta));

    public static Rectangle FromSize(Size size) =>
        new(0, 0, size.Width, size.Height);

    public static Rectangle FromCorners(Position topLeft, Position bottomRight) =>
        new(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
};

internal static class Operators
{
    public static Rectangle CalculateBoundingBox(this IEnumerable<Rectangle> rectangles) =>
        rectangles.ToArray().CalculateBoundingBox();

    public static Rectangle CalculateBoundingBox(this IReadOnlyCollection<Rectangle> rectangles)
    {
        float x = rectangles.Select(r => r.X).Min();
        float y = rectangles.Select(r => r.Y).Min();
        float right = rectangles.Select(r => r.Right).Max();
        float bottom = rectangles.Select(r => r.Bottom).Max();

        return new Rectangle(x, y, right - x, bottom - y);
    }
}