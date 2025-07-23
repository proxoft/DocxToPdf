using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record TableLayout(
    CellLayout[] Cells,
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition
) : Layout(BoundingBox, Borders, Partition), IComposedLayout
{
    public static readonly TableLayout Empty = new(
        [],
        Rectangle.Empty,
        Borders.None,
        LayoutPartition.StartEnd
    );

    public IEnumerable<Layout> InnerLayouts => this.Cells;
}