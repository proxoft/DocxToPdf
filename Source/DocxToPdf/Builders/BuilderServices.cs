using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Builders.Styles;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Builders;

internal class BuilderServices(
    ModelIdFactory modelIdFactory,
    StyleFactory styleFactory)
{
    public BuilderServices(MainDocumentPart mainDocumentPart) : this(new ModelIdFactory(), StyleFactory.Create(mainDocumentPart))
    {
    }

    public ModelIdFactory IdFactory { get; } = modelIdFactory;

    public StyleFactory Styles { get; } = styleFactory;
}
