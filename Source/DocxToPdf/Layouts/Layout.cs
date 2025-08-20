using System.Collections.Generic;
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
    public static T SetOffset<T>(this T layout, Position offset) where T: Layout =>
        layout with
        {
            BoundingBox = layout.BoundingBox.MoveTo(offset)
        };
}