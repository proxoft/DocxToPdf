using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal record LayoutingResult(
    Layout[] Layouts,
    LastProcessed LastProcessed,
    ModelReference ContinueFromElement,
    Rectangle RemainingDrawingArea,
    ResultStatus Status)
{
    public static readonly LayoutingResult None = new([], LastProcessed.None, ModelReference.None, Rectangle.Empty, ResultStatus.RequestDrawingArea);
}
