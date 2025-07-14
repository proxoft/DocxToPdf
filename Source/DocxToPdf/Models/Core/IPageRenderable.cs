using Proxoft.DocxToPdf.Core.Rendering;

namespace Proxoft.DocxToPdf.Models.Core;

internal interface IPageRenderable
{
    void Render(IRendererPage page);
}
