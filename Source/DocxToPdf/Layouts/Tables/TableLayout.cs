using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record TableLayout(
    CellLayout[] Cells,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(new ModelReference([]), BoundingBox, Borders), IComposedLayout
{
    public static readonly TableLayout Empty = new(
        [],
        Rectangle.Empty,
        Borders.None
    );

    public IEnumerable<Layout> InnerLayouts => this.Cells;
}