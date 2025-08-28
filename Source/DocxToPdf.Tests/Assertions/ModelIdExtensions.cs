using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class ModelIdExtensions
{
    public static ModelId AsCellId(this int id) => new ModelId("cel", id);
}
