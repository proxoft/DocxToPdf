using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record CellLayout(
    ModelId ModelId,
    Layout[] ParagraphsOrTables,
    Rectangle BoundingBox,
    Borders Borders,
    GridPosition GridPosition,
    LayoutPartition Partition
) : Layout(ModelId, BoundingBox, Borders, Partition), IComposedLayout
{
    public static readonly CellLayout Empty = new(
        ModelId.None,
        [],
        Rectangle.Empty,
        Borders.None,
        GridPosition.None,
        LayoutPartition.StartEnd
    );

    public IEnumerable<Layout> InnerLayouts => this.ParagraphsOrTables;
}
