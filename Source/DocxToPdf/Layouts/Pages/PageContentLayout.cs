using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts.Headers;
using Proxoft.DocxToPdf.Layouts.Sections;

namespace Proxoft.DocxToPdf.Layouts.Pages;

internal record PageContentLayout(
    HeaderLayout Header,
    SectionLayout[] Sections,
    Rectangle BoundingBox) : Layout(ModelId.None, BoundingBox, Borders.None, LayoutPartition.StartEnd), IComposedLayout
{
    public static readonly PageContentLayout Empty = new(HeaderLayout.None, [], Rectangle.Empty);

    public IEnumerable<Layout> InnerLayouts => [this.Header,  ..this.Sections];
}
