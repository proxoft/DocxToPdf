using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts;

internal record Layout(
    ModelId ModelId,
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition
);

internal sealed record NoLayout : Layout
{
    public static readonly NoLayout Instance = new();

    private NoLayout() : base(ModelId.None, Rectangle.Empty, Borders.None, LayoutPartition.StartEnd)
    {
    }
}


internal interface IComposedLayout
{
    IEnumerable<Layout> InnerLayouts { get; }
}

internal interface IIdLayout
{
    ModelId ModelId { get; }

    bool IsNotEmpty => this.ModelId != ModelId.None;
}

internal static class LayoutOperators
{
    public static Rectangle CalculateBoundingBox(this IEnumerable<Layout> layouts) =>
        layouts
            .Select(l => l.BoundingBox)
            .CalculateBoundingBox();

    public static Rectangle CalculateBoundingBox(this IEnumerable<Layout> layouts, Rectangle ifEmpty) =>
        layouts
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(ifEmpty)
            .CalculateBoundingBox();

    public static T SetOffset<T>(this T layout, Position offset) where T: Layout =>
        layout with
        {
            BoundingBox = layout.BoundingBox.MoveTo(offset)
        };
}