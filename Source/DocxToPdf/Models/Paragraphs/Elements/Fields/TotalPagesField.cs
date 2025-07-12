using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements.Fields;

internal class TotalPagesField(TextStyle textStyle) : Field(textStyle)
{
    private PageVariables _variables = PageVariables.Empty;

    protected override string GetContent() =>
        _variables.TotalPages.ToString();

    protected override void UpdateCore(PageVariables variables)
    {
        _variables = variables;
    }
}
