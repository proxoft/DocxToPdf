using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts.Footers;
using Proxoft.DocxToPdf.Layouts.Headers;
using Proxoft.DocxToPdf.Layouts.Sections;

namespace Proxoft.DocxToPdf.Layouts.Pages;

internal record PageContentLayout(
    HeaderLayout Header,
    SectionLayout[] Sections,
    FooterLayout Footer,
    Rectangle BoundingBox) : Layout(ModelId.None, BoundingBox, Borders.None, LayoutPartition.StartEnd), IComposedLayout
{
    public static readonly PageContentLayout Empty = new(HeaderLayout.None, [], FooterLayout.None, Rectangle.Empty);

    public IEnumerable<Layout> InnerLayouts => [this.Header,  ..this.Sections, this.Footer];
}
