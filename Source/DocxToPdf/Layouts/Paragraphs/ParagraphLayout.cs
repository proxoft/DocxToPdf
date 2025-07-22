using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record ParagraphLayout(
    LineLayout[] Lines,
    Rectangle BoundingBox,
    Borders Borders
) : Layout(BoundingBox, Borders), IComposedLayout
{
    public static readonly ParagraphLayout Empty = new(
        [],
        Rectangle.Empty,
        Borders.None
    );

    public IEnumerable<Layout> InnerLayouts => this.Lines;
}
