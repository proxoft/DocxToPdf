using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Documents.Sections;

internal record PageConfiguration(
    PageMargin Margin,
    Size Size,
    Orientation Orientation)
{
    public static readonly PageConfiguration None = new(PageMargin.None, Size.Zero, Orientation.Portrait);
}
