using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal sealed record ImageLayout(
    ModelId Id,
    byte[] Data,
    Size Size,
    float BaselineOffset,
    Rectangle BoundingBox,
    float LineBaseLineOffset,
    TextStyle TextStyle,
    Borders Borders) : ElementLayout(Id, Size, BaselineOffset, BoundingBox, LineBaseLineOffset, Borders, LayoutPartition.StartEnd)
{
    public override TextStyle GetTextStyle() => this.TextStyle;
}
