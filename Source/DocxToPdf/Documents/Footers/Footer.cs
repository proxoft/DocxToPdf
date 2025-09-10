using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Documents.Footers;

internal record class Footer(
    ModelId Id,
    Model[] ParagraphsOrTables,
    Borders Borders) : Model(Id)
{
    public static readonly Footer None = new(ModelId.None, [], Borders.None);
}
