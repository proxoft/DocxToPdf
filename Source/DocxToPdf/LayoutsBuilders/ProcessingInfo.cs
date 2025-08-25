namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal enum ProcessingInfo
{
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
