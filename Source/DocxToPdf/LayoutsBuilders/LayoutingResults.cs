using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal enum ResultStatus
{
    Finished,
    NewPageRequired,
    RequestDrawingArea,
    IgnoreAndRequestDrawingArea,
    ReconstructRequired
}

internal abstract record LayoutingResult(
    ModelId ModelId,
    Rectangle RemainingDrawingArea,
    ResultStatus Status)
{
    public abstract IEnumerable<Layout> Layouts { get; }
}

internal sealed record NoLayoutingResult : LayoutingResult
{
    private NoLayoutingResult() : base(ModelId.None, Rectangle.Empty, ResultStatus.Finished)
    {
    }

    public override IEnumerable<Layout> Layouts => [];

    public static NoLayoutingResult Create(Rectangle remainingDrawingArea) =>
        new() { RemainingDrawingArea = remainingDrawingArea };
}

internal record SectionLayoutingResult(
    ModelId ModelId,
    SectionLayout SectionLayout,
    LayoutingResult LastModelLayoutingResult, // TableOrParagraph
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(ModelId, RemainingDrawingArea, Status)
{
    public static readonly SectionLayoutingResult None = new(
        ModelId.None,
        SectionLayout.Empty,
        NoLayoutingResult.Create(Rectangle.Empty),
        Rectangle.Empty,
        ResultStatus.Finished
    );

    public override IEnumerable<Layout> Layouts => [this.SectionLayout];
}

internal record ParagraphLayoutingResult(
    ModelId ModelId,
    ParagraphLayout ParagraphLayout,
    ModelId LastProcessedModelId,
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(ModelId, RemainingDrawingArea, Status)
{
    public static ParagraphLayoutingResult None =>
        new(ModelId.None, ParagraphLayout.Empty, ModelId.None, Rectangle.Empty, ResultStatus.Finished);

    public override IEnumerable<Layout> Layouts => [this.ParagraphLayout];
}

internal record CellLayoutingResult(
    ModelId ModelId,
    int Order,
    CellLayout CellLayout,
    GridPosition GridPosition,
    LayoutingResult LastModelLayoutingResult, // TableOrParagraph
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(ModelId, RemainingDrawingArea, Status)
{
    public static readonly CellLayoutingResult None = new(
        ModelId.None,
        -1,
        CellLayout.Empty,
        new GridPosition(0, 0, 0, 0),
        NoLayoutingResult.Create(Rectangle.Empty),
        Rectangle.Empty,
        ResultStatus.Finished
    );

    public override IEnumerable<Layout> Layouts => [this.CellLayout];
}

internal record TableLayoutingResult(
    ModelId ModelId,
    TableLayout TableLayout,
    GridLayout Grid,
    CellLayoutingResult[] CellsLayoutingResults,
    Rectangle RemainingDrawingArea,
    ResultStatus Status) : LayoutingResult(ModelId, RemainingDrawingArea, Status)
{
    public static TableLayoutingResult None = new(
        ModelId.None,
        TableLayout.Empty,
        GridLayout.Empty,
        [],
        Rectangle.Empty,
        ResultStatus.Finished);

    public override IEnumerable<Layout> Layouts => [this.TableLayout];
}

internal static class LayoutingResultFunctions
{
    public static T AsResultOfModel<T>(this LayoutingResult result, ModelId forModel, T ifNone) where T : LayoutingResult =>
        result.ModelId == forModel
            ? (T)result
            : ifNone;
}
