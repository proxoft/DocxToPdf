using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record LineLayout(
    Layout[] Words,
    bool IsLastLineOfParagraph,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(BoundingBox, Borders), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.Words;
}
