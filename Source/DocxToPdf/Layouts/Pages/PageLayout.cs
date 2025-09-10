using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Pages;

internal record PageLayout(
    Rectangle BoundingBox,
    PageContentLayout PageContent,
    Orientation Orientation
) : Layout(ModelId.None, BoundingBox, Borders.None, LayoutPartition.StartEnd)
{
    public static readonly PageLayout None = new(Rectangle.Empty, PageContentLayout.Empty, Orientation.Portrait);
}
