using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;

namespace Proxoft.DocxToPdf.Layouts.Tables;

internal record CellLayout(
    ModelReference Source,
    Layout[] ParagraphsOrTables,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(Source, BoundingBox, Borders), IComposedLayout
{
    public static readonly CellLayout Empty = new(
        new ModelReference([]),
        [],
        Rectangle.Empty,
        Borders.None
    );

    public IEnumerable<Layout> InnerLayouts => this.ParagraphsOrTables;
}
