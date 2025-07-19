using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record CellLayout(
    ModelReference Source,
    Layout[] ParagraphsOrTables,
    Rectangle BoundingBox
) : Layout(Source, BoundingBox), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.ParagraphsOrTables;
}
