using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record TableLayout(
    ModelReference Source,
    CellLayout[] Cells,
    Rectangle BoundingBox
) : Layout(Source, BoundingBox), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.Cells;
}