using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record CellLayout(
    Layout[] ParagraphsOrTables,
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition
) : Layout(BoundingBox, Borders, Partition), IComposedLayout
{
    public static readonly CellLayout Empty = new(
        [],
        Rectangle.Empty,
        Borders.None,
        LayoutPartition.StartEnd
    );

    public IEnumerable<Layout> InnerLayouts => this.ParagraphsOrTables;
}
