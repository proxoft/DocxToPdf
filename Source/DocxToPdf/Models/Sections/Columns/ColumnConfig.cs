namespace Proxoft.DocxToPdf.Models.Sections.Columns;

internal class ColumnConfig(double width, double space)
{
    public double Width { get; } = width;

    public double Space { get; } = space;
}
