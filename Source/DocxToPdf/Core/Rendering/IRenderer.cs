using Proxoft.DocxToPdf.Core.Pages;
using Proxoft.DocxToPdf.Core.Structs;

namespace Proxoft.DocxToPdf.Core.Rendering;

internal interface IRenderer
{
    RenderingOptions Options { get; }

    void CreatePage(PageNumber pageNumber, PageConfiguration configuration);

    IRendererPage GetPage(PageNumber pageNumber);

    IRendererPage GetPage(PageNumber pageNumber, Point offsetRendering);
}
