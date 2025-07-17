namespace Proxoft.DocxToPdf.Documents.Styles.Borders;

internal record BorderStyle(Color Color, float Width, LineStyle LineStyle)
{
    public static readonly BorderStyle None = new(Color.Empty, 0, LineStyle.Solid);
}