using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record EmptyLayout(
    Rectangle BoundingBox,
    TextStyle TextStyle
) : ElementLayout(Size.Zero, 0, BoundingBox, 0, Borders.None, LayoutPartition.StartEnd)
{
    public override TextStyle GetTextStyle() => this.TextStyle;
}
