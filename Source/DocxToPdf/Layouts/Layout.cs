using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts;

internal record Layout(
    ModelReference Source,
    Rectangle BoundingBox
);
