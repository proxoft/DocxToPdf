using System;

namespace Proxoft.DocxToPdf.Documents.Common;

internal record Size(float Width, float Height)
{
    public static readonly Size Zero = new(0, 0);

    public Size DecreaseHeight(float height) =>
        new(this.Width, Math.Max(0, this.Height - height));

    public Size DecreaseWidth(float delta) =>
        new(Math.Max(0, this.Width - delta), this.Height);

    public bool FitsIn(Size other) =>
        this.Width <= other.Width && this.Height <= other.Height;
}