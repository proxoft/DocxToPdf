using Proxoft.DocxToPdf.Core.Pages;

namespace Proxoft.DocxToPdf.Core
{
    internal interface IRenderer
    {
        RenderingOptions Options { get; }

        void CreatePage(PageNumber pageNumber, PageConfiguration configuration);

        IRendererPage GetPage(PageNumber pageNumber);

        IRendererPage GetPage(PageNumber pageNumber, Point offsetRendering);
    }
}
