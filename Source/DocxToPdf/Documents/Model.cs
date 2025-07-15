namespace Proxoft.DocxToPdf.Documents;

internal abstract record Model(ModelId Id)
{
}

internal record IgnoredModel(ModelId Id) : Model(Id);
