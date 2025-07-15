using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Builders;

internal class BuilderServices(
    ModelIdFactory modelIdFactory)
{
    public BuilderServices() : this(new ModelIdFactory())
    {
    }

    public ModelIdFactory IdFactory { get; } = modelIdFactory;
}
