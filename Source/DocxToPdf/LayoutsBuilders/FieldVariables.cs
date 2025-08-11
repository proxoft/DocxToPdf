namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal record FieldVariables(int CurrentPage, int TotalPages)
{
    public static readonly FieldVariables None = new(0, 0);
}
