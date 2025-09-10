namespace Proxoft.DocxToPdf.Documents.Common;

internal record Position(float X, float Y)
{
    public static readonly Position Zero = new(0, 0);

    public Position Shift(float deltaX, float deltaY) =>
        new(this.X + deltaX, this.Y + deltaY);

    public Position ShiftX(float delta) =>
        this.Shift(delta, 0);

    public Position ShiftY(float delta) =>
        this.Shift(0, delta);
}