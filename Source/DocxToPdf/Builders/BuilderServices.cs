using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Builders.Services;
using Proxoft.DocxToPdf.Builders.Styles;
using Proxoft.DocxToPdf.Core.Images;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Builders;

internal class BuilderServices(
    ModelIdFactory modelIdFactory,
    StyleFactory styleFactory,
    ImageAccessor imageAccessor,
    HeaderFooterAccessor headerFooterAccessor)
{
    public BuilderServices(MainDocumentPart mainDocumentPart) : this(
        new ModelIdFactory(),
        StyleFactory.Create(mainDocumentPart),
        ImageAccessor.Create(mainDocumentPart),
        HeaderFooterAccessor.Create(mainDocumentPart)
    )
    {
    }

    public ModelIdFactory IdFactory { get; } = modelIdFactory;

    public StyleFactory Styles { get; } = styleFactory;

    public ImageAccessor ImageAccessor { get; } = imageAccessor;

    public HeaderFooterAccessor HeaderFooterAccessor { get; } = headerFooterAccessor;

    public BuilderServices ForTable(Word.TableProperties tableProperties)
    {
        StyleFactory sf = this.Styles.ForTable(tableProperties);
        return new BuilderServices(this.IdFactory, sf, this.ImageAccessor, this.HeaderFooterAccessor);
    }
}
