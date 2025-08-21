namespace Proxoft.DocxToPdf.Documents;

internal abstract record Model(ModelId Id)
{
}

internal record NoneModel() : Model(ModelId.None)
{
    public static readonly NoneModel Instance = new();
}

internal record IgnoredModel(ModelId Id) : Model(Id);
