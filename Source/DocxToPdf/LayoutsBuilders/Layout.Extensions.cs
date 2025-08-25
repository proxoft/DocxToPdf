using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class LayoutExtensions
{
    public static bool IsNotEmpty(this Layout layout) => layout.ModelId != ModelId.None;

    public static bool IsUpdateFinished(this Layout[] originalLayouts, Layout[] updatedLayouts) =>
        originalLayouts.Length == 0 && updatedLayouts.Length == 0
        || (
            originalLayouts.Length == updatedLayouts.Length
            && originalLayouts.Last().ModelId == updatedLayouts.Last().ModelId
            && updatedLayouts.Last().Partition.IsFinished()
        );

    public static ParagraphLayout TryFindParagraphLayout(this IComposedLayout parent, ModelId paragraphId) =>
        parent.InnerLayouts.OfType<ParagraphLayout>().SingleOrDefault(p => p.ModelId == paragraphId, ParagraphLayout.Empty);

    public static TableLayout TryFindTableLayout(this IComposedLayout parent, ModelId tableId) =>
        parent.InnerLayouts.OfType<TableLayout>().SingleOrDefault(p => p.ModelId == tableId, TableLayout.Empty);
}
