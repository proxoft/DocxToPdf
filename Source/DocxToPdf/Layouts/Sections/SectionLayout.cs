using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Sections;

internal record SectionLayout(
    ModelId ModelId,
    Layout[] Layouts,
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition) : Layout(BoundingBox, Borders, Partition), IComposedLayout
{
    public static readonly SectionLayout Empty = new(
        ModelId.None,
        [],
        Rectangle.Empty,
        Borders.None,
        LayoutPartition.StartEnd
    );

    public IEnumerable<Layout> InnerLayouts => this.Layouts;
}
