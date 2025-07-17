using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record LineLayout(
    ModelReference Source,
    Layout[] Words,
    Rectangle BoundingBox
) : Layout(Source, BoundingBox), IComposedLayout
{
    public IEnumerable<Layout> InnerLayouts => this.Words;

    public static LineLayout New(ModelReference forParagraph) =>
        new(forParagraph, [], Rectangle.Empty);
}
