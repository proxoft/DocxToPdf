using System.Linq;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal record LastProcessed(ModelId[] Models)
{
    public static readonly LastProcessed None = new([]);

    public ModelId Current => this.Models.FirstOrDefault(ModelId.None);

    public LastProcessed Append(ModelId modelId) =>
        new([.. this.Models, modelId]);

    public LastProcessed Childs() =>
        new([.. this.Models.Skip(1)]);
};
