using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.Documents;

internal sealed record DocumentModel(
    Section[] Sections
)
{
    public static readonly DocumentModel Null = new([]);
};
