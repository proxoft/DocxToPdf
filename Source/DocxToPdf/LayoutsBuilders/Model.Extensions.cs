using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class ModelExtensions
{
    public static Model Next(this Model[] models, ModelId current) =>
        models
            .SkipWhile(m => m.Id != current)
            .Skip(1)
            .FirstOrDefault(NoneModel.Instance);

    

    public static TModel[] SkipProcessed<TModel>(this IEnumerable<TModel> models, LayoutingResult lastResult) where TModel : Model
    {
        bool lastWasFinished = lastResult.Status == ResultStatus.Finished;
        return [..models.SkipProcessed(lastResult.ModelId, lastWasFinished)];
    }

    public static IEnumerable<TModel> SkipProcessed<TModel>(this IEnumerable<TModel> models, ModelId lastProcessed) where TModel : Model =>
        lastProcessed == ModelId.None
            ? models
            : models.SkipWhile(e => e.Id != lastProcessed);

    public static IEnumerable<TModel> SkipProcessed<TModel>(this IEnumerable<TModel> models, ModelId lastProcessed, bool finished) where TModel : Model =>
        finished
            ? models.SkipProcessed(lastProcessed).Skip(lastProcessed == ModelId.None ? 0 : 1)
            : models.SkipProcessed(lastProcessed);
}
