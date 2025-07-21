using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts;

internal record Layout(
    ModelReference Source,
    Rectangle BoundingBox,
    Borders Borders
);

internal interface IComposedLayout
{
    IEnumerable<Layout> InnerLayouts { get; }
}
