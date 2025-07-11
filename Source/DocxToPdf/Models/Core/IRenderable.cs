using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Core;

internal interface IRenderable
{
    void Render(IRenderer renderer);
}
