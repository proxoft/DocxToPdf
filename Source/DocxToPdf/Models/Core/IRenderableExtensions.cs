using System.Collections.Generic;
using Proxoft.DocxToPdf.Core.Rendering;

namespace Proxoft.DocxToPdf.Models.Core;

internal static class IRenderableExtensions
{
    public static void Render(this IEnumerable<IRenderable> elements, IRenderer renderer)
    {
        foreach (var e in elements)
        {
            e.Render(renderer);
        }
    }
}
