using System;
using System.Collections.Generic;
using System.Linq;

namespace Proxoft.DocxToPdf.Documents.Common;

internal record Rectangle(float X, float Y, float Width, float Height)
{
    public static readonly Rectangle Empty = new(0,0,0,0);

    public Rectangle(Position position, Size size) : this(position.X, position.Y, size.Width, size.Height)
    {
    }

    public float Left => this.X;

    public float Top => this.Y;

    public float Right => this.X + this.Width;

    public float Bottom => this.Y + this.Height;

    public Position TopLeft => new(this.X, this.Y);

    public Position TopRight => new(this.Right, this.Y);

    public Position BottomLeft => new(this.X, this.Bottom);

    public Position BottomRight => new(this.Right, this.Bottom);

    public Size Size => new(this.Width, this.Height);

    public (Position, Position) LeftLine => (this.BottomLeft, this.TopLeft);

    public (Position, Position) TopLine => (this.TopLeft, this.TopRight);

    public (Position, Position) RightLine => (this.TopRight, this.BottomRight);

    public (Position, Position) BottomLine => (this.BottomRight, this.BottomLeft);

    public Rectangle MoveTo(Position position) =>
        new(position.X, position.Y, this.Width, this.Height);

    public Rectangle MoveX(float deltaX) =>
        new(this.TopLeft.X + deltaX, this.TopLeft.Y, this.Width, this.Height);

    public Rectangle MoveY(float deltaY) =>
        new(this.TopLeft.X, this.TopLeft.Y + deltaY, this.Width, this.Height);

    public Rectangle SetWidth(float width) =>
        new(this.X, this.Y, width, this.Height);

    public Rectangle SetHeight(float height) =>
        new(this.X, this.Y, this.Width, height);

    public Rectangle CropFromLeft(float delta) =>
        FromCorners(new Position(this.X + delta, this.Y), this.BottomRight);

    public Rectangle CropFromTop(float delta) =>
        FromCorners(new Position(this.X, this.Y + delta), this.BottomRight);

    public Rectangle CropFromRight(float delta) =>
        FromCorners(this.TopLeft, new Position(this.BottomRight.X - delta, this.BottomRight.Y));

    public Rectangle CropFromBottom(float delta) =>
        FromCorners(this.TopLeft, new Position(this.BottomRight.X, this.BottomRight.Y - delta));

    public Rectangle CropWidth(float width) =>
        new(this.X, this.Y, Math.Min(this.Width, width), this.Height);

    public Rectangle Clip(Padding padding) =>
       this.CropFromLeft(padding.Left)
           .CropFromTop(padding.Top)
           .CropFromRight(padding.Right)
           .CropFromBottom(padding.Bottom);

    public Rectangle ExpandHeight(float delta) =>
        new Rectangle(this.TopLeft, new Size(this.Width, this.Height + delta));

    public Rectangle Expand(Padding padding) =>
       this.CropFromLeft(-padding.Left)
           .CropFromTop(-padding.Top)
           .CropFromRight(-padding.Right)
           .CropFromBottom(-padding.Bottom);

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