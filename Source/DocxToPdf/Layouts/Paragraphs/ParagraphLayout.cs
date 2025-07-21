using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record ParagraphLayout(
    ModelReference Source,
    LineLayout[] Lines,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(Source, BoundingBox, Borders), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.Lines;
}
