using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models
{
    internal interface IPageRenderable
    {
        void Render(IRendererPage page);
    }
}
