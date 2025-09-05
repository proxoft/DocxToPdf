using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Footers;

internal record FooterLayout(
    ModelId ModelId,
    Layout[] ParagraphsAndTables,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(ModelId, BoundingBox, Borders, LayoutPartition.StartEnd), IComposedLayout
{
    public static readonly FooterLayout None = new(ModelId.None, [], Rectangle.Empty, Borders.None);

    public IEnumerable<Layout> InnerLayouts => this.ParagraphsAndTables;
}