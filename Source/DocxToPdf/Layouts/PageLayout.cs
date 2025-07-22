using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts;

internal record PageLayout(
    Rectangle BoundingBox,
    Rectangle DrawingArea,
    Layout[] Content,
    PageConfiguration Configuration,
    Borders Borders
) : Layout(BoundingBox, Borders);
