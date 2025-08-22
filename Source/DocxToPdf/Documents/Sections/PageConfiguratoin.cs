using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Documents.Sections;

internal record PageConfiguration(
    PageMargin Margin,
    Size Size,
    Orientation Orientation)
{
    public static readonly PageConfiguration None = new(PageMargin.None, Size.Zero, Orientation.Portrait);
}
