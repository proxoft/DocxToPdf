using System.Text;

namespace Proxoft.DocxToPdf
{
    public class PdfGenerator
    {
        static PdfGenerator()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
