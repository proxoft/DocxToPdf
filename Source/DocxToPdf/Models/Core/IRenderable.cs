using Proxoft.DocxToPdf.Core.Rendering;

namespace Proxoft.DocxToPdf.Models.Core;

internal interface IRenderable
{
    void Render(IRenderer renderer);
}
