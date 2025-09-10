using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Documents.Headers;

internal record Header(
    ModelId Id,
    Model[] ParagraphsOrTables,
    Borders Borders) : Model(Id)
{
    public static readonly Header None = new(ModelId.None, [], Borders.None);
}
