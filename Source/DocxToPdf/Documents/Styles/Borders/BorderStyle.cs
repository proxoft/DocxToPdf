namespace Proxoft.DocxToPdf.Documents.Styles.Borders;

internal record BorderStyle(Color Color, float Width, LineStyle LineStyle)
{
    public static readonly BorderStyle None = new(Color.Empty, 0, LineStyle.Solid);

    public static readonly BorderStyle SolidBlack = new(new Color("000000"), 0.5f, LineStyle.Solid);
}