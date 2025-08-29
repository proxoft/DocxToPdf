using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class ModelIdExtensions
{
    public static ModelId AsSectionId(this int id) => new("sct", id);

    public static ModelId AsCellId(this int id) => new("cel", id);
}
