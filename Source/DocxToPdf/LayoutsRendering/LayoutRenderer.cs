using System.Collections.Generic;
using System.Text;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsRendering.Renderers;

namespace Proxoft.DocxToPdf.LayoutsRendering;

internal static class LayoutRenderer
{
    static LayoutRenderer()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static PdfDocument CreatePdf(PageLayout[] pages, RenderOptions options)
    {
        PdfDocument pdf = new();

        foreach (PageLayout page in pages)
        {
            page.CreatePdfPage(pdf, options);
        }

        return pdf;
    }

    private static void CreatePdfPage(this PageLayout page, PdfDocument pdfDocument, RenderOptions options)
    {
        PageOrientation orientation = page.Orientation switch
        {
            Documents.Shared.Orientation.Portrait => PageOrientation.Portrait,
            Documents.Shared.Orientation.Landscape => PageOrientation.Landscape,
            _ => PageOrientation.Portrait,
        };

        PdfPage pp = new()
        {
            Orientation = orientation,
            Width = page.BoundingBox.Size.Width,
            Height = page.BoundingBox.Size.Height
        };

        pdfDocument.AddPage(pp);

        XGraphics graphics = XGraphics.FromPdfPage(pp);
        page.Render(graphics, options);
    }

    private static void Render(this PageLayout page, XGraphics graphics, RenderOptions options) => 
        page.PageContent.Render(Position.Zero, graphics, options);

    private static void Render(this IEnumerable<Layout> layouts, Position offset, XGraphics graphics, RenderOptions options)
    {
        foreach (Layout layout in layouts)
        {
            layout.Render(offset, graphics, options);
        }
    }

    private static void Render(this Layout layout, Position offset, XGraphics graphics, RenderOptions options)
    {
        switch (layout)
        {
            case ImageLayout imageLayout:
                imageLayout.RenderImage(offset, graphics);
                break;
            case ElementLayout element:
                element.RenderText(offset, graphics, options);
                break;
            case IComposedLayout composedLayout:
                composedLayout.InnerLayouts.Render(offset.Shift(layout.BoundingBox.TopLeft.X, layout.BoundingBox.TopLeft.Y), graphics, options);
                break;
        }

        layout.RenderBorder(offset, graphics);
        layout.RenderSpecialCharacter(offset.Shift(layout.BoundingBox.TopLeft.X, layout.BoundingBox.TopLeft.Y), graphics, options);
        layout.RenderDebuggingBorder(offset, graphics, options);
    }
}
