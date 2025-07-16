namespace Proxoft.DocxToPdf.Documents.Sections;

internal record Section(
    ModelId Id,
    SectionProperties Properties,
    Model[] Elements
) : Model(Id);