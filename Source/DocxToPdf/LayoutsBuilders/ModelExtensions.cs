using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class ModelExtensions
{
    public static IEnumerable<TModel> SkipProcessed<TModel>(this IEnumerable<TModel> models, ModelId lastProcessed) where TModel : Model =>
        lastProcessed == ModelId.None
            ? models
            : models.SkipWhile(e => e.Id != lastProcessed);
}
