using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record LineLayout(
    ModelReference Source,
    Layout[] Words,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(Source, BoundingBox, Borders), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.Words;

    public static LineLayout New(ModelReference forParagraph) =>
        new(forParagraph, [], Rectangle.Empty, Borders.None);
}
