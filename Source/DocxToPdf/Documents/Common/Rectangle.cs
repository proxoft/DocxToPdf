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

    public float Botton => this.Y + this.Height;

    public Position Position => new(this.X, this.Y);

    public Size Size => new(this.Width, this.Height);

    public Rectangle CropFromLeft(float delta) =>
        new(this.X + delta, this.Y, this.Width, this.Height);

    public Rectangle CropFromTop(float delta) =>
        new(this.X, this.Y + delta, this.Width, this.Height);

    public Rectangle CropFromRight(float delta) =>
        new(this.X, this.Y, this.Width - delta, this.Height);

    public Rectangle CropFromBottom(float delta) =>
        new(this.X, this.Y, this.Width, this.Height - delta);

    public static Rectangle FromSize(Size size) =>
        new(0, 0, size.Width, size.Height);
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
        float bottom = rectangles.Select(r => r.Botton).Max();

        return new Rectangle(x, y, right - x, bottom - y);
    }
}