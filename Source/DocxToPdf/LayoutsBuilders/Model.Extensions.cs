using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class ModelExtensions
{
    public static Model Next(this Model[] models, ModelId current) =>
        models
            .SkipWhile(m => m.Id != current)
            .Skip(1)
            .FirstOrDefault(NoneModel.Instance);

    public static T Find<T>(this IEnumerable<Model> models, ModelId id) where T : Model =>
        models.OfType<T>().Single(e => e.Id == id);

    public static IEnumerable<TModel> SkipProcessed<TModel>(this IEnumerable<TModel> models, ModelId lastProcessed) where TModel : Model =>
        lastProcessed == ModelId.None
            ? models
            : models.SkipWhile(e => e.Id != lastProcessed);

    public static IEnumerable<TModel> SkipProcessed<TModel>(this IEnumerable<TModel> models, ModelId lastProcessed, bool finished) where TModel : Model =>
        finished
            ? models.SkipProcessed(lastProcessed).Skip(lastProcessed == ModelId.None ? 0 : 1)
            : models.SkipProcessed(lastProcessed);
}
