using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record FieldLayout(
    string Content,
    Size Size,
    float BaselineOffset,
    Rectangle BoundingBox,
    float LineBaseLineOffset,
    Borders Borders,
    TextStyle TextStyle,
    LayoutPartition Partition) : ElementLayout(Size, BaselineOffset, BoundingBox, LineBaseLineOffset, Borders, Partition)
{
    public override TextStyle GetTextStyle() => this.TextStyle;
}
