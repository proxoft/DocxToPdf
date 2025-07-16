using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal record LayoutingResult(
    Layout[] Layouts,
    ModelReference ContinueFromElement,
    Rectangle RemainingDrawingArea)
{
    public static readonly LayoutingResult None = new([], ModelReference.None, Rectangle.Empty);

    public bool IsFinished => this.ContinueFromElement.IsNone;
}
