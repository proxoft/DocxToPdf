namespace Proxoft.DocxToPdf.Documents.Sections;

internal class ColumnConfig(float width, float space)
{
    public float Width { get; } = width;

    public float Space { get; } = space;
}
