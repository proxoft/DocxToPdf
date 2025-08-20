using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record ParagraphLayout(
    ModelId ModelId,
    LineLayout[] Lines,
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition
) : Layout(ModelId, BoundingBox, Borders, Partition), IIdLayout, IComposedLayout
{
    public static readonly ParagraphLayout Empty = new(
        ModelId.None,
        [],
        Rectangle.Empty,
        Borders.None,
        LayoutPartition.StartEnd
    );

    public IEnumerable<Layout> InnerLayouts => this.Lines;
}
