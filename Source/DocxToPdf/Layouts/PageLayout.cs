using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts.Sections;

namespace Proxoft.DocxToPdf.Layouts;

internal record PageLayout(
    Rectangle BoundingBox,
    PageContentLayout PageContent,
    PageConfiguration Configuration
) : Layout(ModelId.None, BoundingBox, Borders.None, LayoutPartition.StartEnd)
{
    public static readonly PageLayout None = new(Rectangle.Empty, PageContentLayout.Empty, PageConfiguration.None);
}

internal record PageContentLayout(
    SectionLayout[] Sections,
    Rectangle BoundingBox) : Layout(ModelId.None, BoundingBox, Borders.None, LayoutPartition.StartEnd), IComposedLayout
{
    public static readonly PageContentLayout Empty = new([], Rectangle.Empty);

    public IEnumerable<Layout> InnerLayouts => this.Sections;
}
