using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Tables;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record TableLayout(
    ModelReference Source,
    CellLayout[] Cells,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(Source, BoundingBox, Borders), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.Cells;
}