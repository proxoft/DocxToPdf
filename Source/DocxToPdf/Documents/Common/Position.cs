namespace Proxoft.DocxToPdf.Documents.Common;

internal record Position(float X, float Y)
{
    public Position ShiftX(float delta) =>
        new(this.X + delta, this.Y);

    public Position ShiftY(float delta) =>
        new(this.X, this.Y + delta);
}