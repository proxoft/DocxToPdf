namespace Proxoft.DocxToPdf.Documents.Common;

internal record Size(float Width, float Height)
{
    public static readonly Size Zero = new(0, 0);

    public bool FitsIn(Size other) =>
        this.Width <= other.Width && this.Height <= other.Height;
}