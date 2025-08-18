using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record EmptyLayout(
    ModelId Id,
    Rectangle BoundingBox,
    TextStyle TextStyle
) : ElementLayout(Id, Size.Zero, 0, BoundingBox, 0, Borders.None, LayoutPartition.StartEnd)
{
    public override TextStyle GetTextStyle() => this.TextStyle;
}
