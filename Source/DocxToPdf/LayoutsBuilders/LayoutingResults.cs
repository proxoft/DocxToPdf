using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal enum ResultStatus
{
    Finished,
    RequestDrawingArea
}

internal record LayoutingResult(
    Layout[] Layouts,
    Rectangle RemainingDrawingArea,
    ResultStatus Status);

internal record NoLayoutingResult(Layout[] Layouts, Rectangle RemainingDrawingArea, ResultStatus Status) : LayoutingResult(Layouts, RemainingDrawingArea, Status)
{
    public static readonly NoLayoutingResult Instance = new([], Rectangle.Empty, ResultStatus.Finished);
}

internal record SectionLayoutingResult(
    Layout[] Layouts,
    ModelId LastProcessedModel,
    LayoutingResult LastModelLayoutingResult, // TableOrParagraph
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(Layouts, RemainingDrawingArea, Status)
{
    public static readonly SectionLayoutingResult None = new(
        [],
        ModelId.None,
        NoLayoutingResult.Instance,
        Rectangle.Empty,
        ResultStatus.Finished
    );
}

internal record ParagraphLayoutingResult(
    Layout[] Layouts,
    ModelId StartFromElementId,
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(Layouts, RemainingDrawingArea, Status)
{
    public static ParagraphLayoutingResult New(Rectangle remainingDrawingArea) =>
        new([], ModelId.None, remainingDrawingArea, ResultStatus.Finished);
}

internal record CellLayoutingResult(
    Layout[] Layouts,
    ModelId LastProcessedModel,
    LayoutingResult LastModelLayoutingResult, // TableOrParagraph
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(Layouts, RemainingDrawingArea, Status);

internal record TableLayoutingResult(
    Layout[] Layouts,
    ModelId LastProcessedElement,
    CellLayoutingResult[] CellsLayoutingResult,
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(Layouts, RemainingDrawingArea, Status)
{
    public static TableLayoutingResult New(Rectangle remainingDrawingArea) =>
        new([], ModelId.None, [], remainingDrawingArea, ResultStatus.Finished);
}
