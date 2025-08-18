using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts;

internal record Layout(
    Rectangle BoundingBox,
    Borders Borders,
    LayoutPartition Partition
);

internal interface IComposedLayout
{
    IEnumerable<Layout> InnerLayouts { get; }
}

internal interface IIdLayout
{
    ModelId ModelId { get; }

    bool IsNotEmpty => this.ModelId != ModelId.None;
}