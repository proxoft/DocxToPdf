namespace Proxoft.DocxToPdf.Documents.Styles;

internal record Color(string Hex)
{
    public static readonly Color Empty = new("");
}
