namespace Proxoft.DocxToPdf.Models.Common;

internal class DocumentVariables(int totalPages)
{
    public int TotalPages { get; } = totalPages;
}
