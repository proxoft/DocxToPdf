using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements.Fields
{
    internal class TotalPagesField : Field
    {
        private PageVariables _variables = PageVariables.Empty;

        public TotalPagesField(TextStyle textStyle) : base(textStyle)
        {
        }

        protected override string GetContent()
            => _variables.TotalPages.ToString();

        protected override void UpdateCore(PageVariables variables)
        {
            _variables = variables;
        }
    }
}
