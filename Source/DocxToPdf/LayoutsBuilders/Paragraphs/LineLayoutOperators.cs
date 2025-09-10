using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class LineLayoutOperators
{
    public static bool ContainsUpdatableField(this LineLayout line) =>
        line.Words.Any(w => w is FieldLayout);

    public static ModelId LastProcessedElementId(this LineLayout[] lines) =>
        lines.Length == 0
            ? ModelId.None
            : lines.Last().LastProcessedElementId();

    private static ModelId LastProcessedElementId(this LineLayout lineLayout) =>
        lineLayout.Words.Length == 0
            ? ModelId.None
            : lineLayout.Words.Last().ModelId;
}
