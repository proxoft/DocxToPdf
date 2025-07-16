using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.Layouts;

internal record PageLayout(
    ModelReference Source,
    Rectangle BoundingBox,
    Rectangle DrawingArea,
    Layout[] Content,
    PageConfiguration Configuration
) : Layout(Source, BoundingBox);
