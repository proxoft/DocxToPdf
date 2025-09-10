using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Sections;

internal record ColumnLayout(
    ModelId ModelId,
    Layout[] ParagraphsOrTables,
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition) : Layout(ModelId, BoundingBox, Borders, Partition), IComposedLayout
{
    public static readonly ColumnLayout None = new(
        ModelId.None,
        [],
        Rectangle.Empty,
        Borders.None,
        LayoutPartition.StartEnd
    );

    public IEnumerable<Layout> InnerLayouts => this.ParagraphsOrTables;
}
