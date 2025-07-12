using Proxoft.DocxToPdf.Core.Rendering;

namespace Proxoft.DocxToPdf.Models;

internal interface IPageRenderable
{
    void Render(IRendererPage page);
}
