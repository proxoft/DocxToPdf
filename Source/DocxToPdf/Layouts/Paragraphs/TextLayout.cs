using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record TextLayout(
    ModelId Id,
    Size Size,
    float BaselineOffset,
    Rectangle BoundingBox,
    float LineBaseLineOffset,
    Text Text,
    Borders Borders,
    LayoutPartition Partition) : ElementLayout(Id, Size, BaselineOffset, BoundingBox, LineBaseLineOffset, Borders, Partition)
{
    public override TextStyle GetTextStyle() => this.Text.TextStyle;
}
