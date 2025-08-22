namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal enum ProcessingInfo
{
    Ignore,
    Done,
    NewPageRequired,
    RequestDrawingArea,
    IgnoreAndRequestDrawingArea,
    ReconstructRequired
}
