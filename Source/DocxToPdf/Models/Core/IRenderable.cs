using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models
{
    internal interface IRenderable
    {
        void Render(IRenderer renderer);
    }
}
