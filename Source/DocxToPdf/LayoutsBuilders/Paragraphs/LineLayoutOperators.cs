using System.Linq;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class LineLayoutOperators
{
    public static bool ContainsUpdatableField(this LineLayout line) =>
        line.Words.Any(w => w is PageNumberLayout);
}
