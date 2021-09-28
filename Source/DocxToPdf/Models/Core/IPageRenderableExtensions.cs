using System.Collections.Generic;
using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models
{
    internal static class IPageRenderableExtensions
    {
        public static void Render(this IEnumerable<IPageRenderable> elements, IRendererPage renderer)
        {
            foreach (var e in elements)
            {
                e.Render(renderer);
            }
        }
    }
}
