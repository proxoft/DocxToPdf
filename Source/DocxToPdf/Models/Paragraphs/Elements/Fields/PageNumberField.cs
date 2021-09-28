using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements.Fields
{
    internal class PageNumberField : Field
    {
        public PageNumberField(TextStyle textStyle) : base(textStyle)
        {
        }

        private PageVariables _variables = PageVariables.Empty;

        protected override string GetContent()
            => ((int)_variables.PageNumber).ToString();

        protected override void UpdateCore(PageVariables variables)
            => _variables = variables;
    }
}
