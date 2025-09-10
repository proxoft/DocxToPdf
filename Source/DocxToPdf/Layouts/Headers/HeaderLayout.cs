using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Headers;

internal record HeaderLayout(
    ModelId ModelId,
    Layout[] ParagraphsAndTables,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(ModelId, BoundingBox, Borders, LayoutPartition.StartEnd), IComposedLayout
{
    public static readonly HeaderLayout None = new(ModelId.None, [], Rectangle.Empty, Borders.None);

    public IEnumerable<Layout> InnerLayouts => this.ParagraphsAndTables;
}
