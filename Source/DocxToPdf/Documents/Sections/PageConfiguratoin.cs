using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Documents.Sections;

internal record PageConfiguration(
    PageMargin Margin,
    Size Size,
    Orientation Orientation);
