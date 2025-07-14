using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements.Fields;

internal class EmptyField(TextStyle textStyle) : Field(textStyle)
{
    protected override string GetContent() => string.Empty;

    protected override void UpdateCore(PageVariables variables)
    {
    }
}
