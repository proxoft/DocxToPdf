using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record SpaceLayout(
    Size Size,
    float BaselineOffset,
    Rectangle BoundingBox,
    float LineBaseLineOffset,
    Borders Borders,
    TextStyle  TextStyle
) : ElementLayout(Size, BaselineOffset, BoundingBox, LineBaseLineOffset, Borders, LayoutPartition.StartEnd)
{
    public override TextStyle GetTextStyle() => this.TextStyle;
}
