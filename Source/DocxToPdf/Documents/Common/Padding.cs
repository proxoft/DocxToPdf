namespace Proxoft.DocxToPdf.Documents.Common;

internal record Padding(float Left, float Top, float Right, float Bottom)
{
    public float Horizontal => this.Left + this.Right;
    public float Vertical => this.Top + this.Bottom;
}