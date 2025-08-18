using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal enum LineDecoration
{
    None,
    Last,
    PageBreak,
}

internal record LineLayout(
    ElementLayout[] Words,
    LineDecoration Decoration,
    Rectangle BoundingBox,
    Borders Borders,
    ElementLayout DecorationText
) : Layout(BoundingBox, Borders, LayoutPartition.StartEnd), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.Words;
}