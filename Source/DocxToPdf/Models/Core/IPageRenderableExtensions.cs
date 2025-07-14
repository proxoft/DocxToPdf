using System.Collections.Generic;
using Proxoft.DocxToPdf.Core.Rendering;

namespace Proxoft.DocxToPdf.Models.Core;

internal static class IPageRenderableExtensions
{
    public static void Render(this IEnumerable<IPageRenderable> elements, IRendererPage renderer)
    {
        foreach (IPageRenderable e in elements)
        {
            e.Render(renderer);
        }
    }
}
