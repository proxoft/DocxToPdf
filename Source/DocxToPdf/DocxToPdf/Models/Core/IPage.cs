using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models
{
    internal interface IPage
    {
        PageNumber PageNumber { get; }

        DocumentVariables DocumentVariables { get; }

        PageConfiguration Configuration { get; }

        PageMargin Margin { get; }

        Rectangle GetPageRegion();

        Rectangle GetContentRegion();

        void SetHorizontalMargins(double left, double right);
        void SetTopMargins(double header, double top);
        void SetBottomMargins(double footer, double bottom);
    }
}
