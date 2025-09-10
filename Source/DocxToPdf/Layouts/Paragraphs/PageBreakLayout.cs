using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

//internal record PageBreakLayout(ModelId Id, TextStyle TextStyle) : ElementLayout(Id, Size.Zero, 0, Rectangle.Empty, 0, Borders.None, LayoutPartition.StartEnd)
//{
//    public override TextStyle GetTextStyle() => this.TextStyle;
//}

internal record BreakLayout(ModelId Id, BreakType BreakType, TextStyle TextStyle) : ElementLayout(Id, Size.Zero, 0, Rectangle.Empty, 0, Borders.None, LayoutPartition.StartEnd)
{
    public override TextStyle GetTextStyle() => this.TextStyle;
}
