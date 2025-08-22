using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class LayoutExtensions
{
    public static ParagraphLayout TryFindParagraphLayout(this IComposedLayout parent, ModelId paragraphId) =>
        parent.InnerLayouts.OfType<ParagraphLayout>().SingleOrDefault(p => p.ModelId == paragraphId, ParagraphLayout.Empty);

    public static TableLayout TryFindTableLayout(this IComposedLayout parent, ModelId tableId) =>
        parent.InnerLayouts.OfType<TableLayout>().SingleOrDefault(p => p.ModelId == tableId, TableLayout.Empty);
}
