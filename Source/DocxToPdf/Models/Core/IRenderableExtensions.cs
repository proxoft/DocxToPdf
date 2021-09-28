using System.Collections.Generic;
using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models
{
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
}
