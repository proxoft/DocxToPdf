using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record TableLayout(
    ModelId ModelId,
    CellLayout[] Cells,
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition
) : Layout(ModelId, BoundingBox, Borders, Partition), IIdLayout, IComposedLayout
{
    public static readonly TableLayout Empty = new(
        ModelId.None,
        [],
        Rectangle.Empty,
        Borders.None,
        LayoutPartition.StartEnd
    );

    public IEnumerable<Layout> InnerLayouts => this.Cells;
}