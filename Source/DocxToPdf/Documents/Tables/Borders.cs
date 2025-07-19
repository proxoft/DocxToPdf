using Proxoft.DocxToPdf.Documents.Styles.Borders;

namespace Proxoft.DocxToPdf.Documents.Tables;

internal record Borders(
    BorderStyle Left,
    BorderStyle Top,
    BorderStyle Right,
    BorderStyle Bottom
)
{
    public static readonly Borders SolidBlack = new(BorderStyle.SolidBlack, BorderStyle.SolidBlack, BorderStyle.SolidBlack, BorderStyle.SolidBlack);
}
