using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts;

internal record Layout(
    ModelReference Source,
    Rectangle BoundingBox
);

internal interface IComposedLayout
{
    IEnumerable<Layout> InnerLayouts { get; }
}
