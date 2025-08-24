namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal enum ProcessingInfo
{
    Ignore,
    Done,
    NewPageRequired,
    RequestDrawingArea,
    IgnoreAndRequestDrawingArea
}

internal enum UpdateInfo
{
    Done,
    ReconstructRequired
}
