using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts.Sections;

namespace Proxoft.DocxToPdf.Layouts;

internal record PageLayout(
    Rectangle BoundingBox,
    Rectangle DrawingArea,
    SectionLayout[] Content,
    PageConfiguration Configuration,
    Borders Borders
) : Layout(ModelId.None, BoundingBox, Borders, LayoutPartition.StartEnd), IComposedLayout
{
    public static readonly PageLayout None = new(Rectangle.Empty, Rectangle.Empty, [], PageConfiguration.None, Borders.None);

    public IEnumerable<Layout> InnerLayouts => this.Content;
}
