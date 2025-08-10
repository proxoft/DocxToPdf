using System.Collections.Generic;
using System.Text;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Layouts;
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
        PageOrientation orientation = page.Configuration.Orientation switch
        {
            Documents.Sections.Orientation.Portrait => PageOrientation.Portrait,
            Documents.Sections.Orientation.Landscape => PageOrientation.Landscape,
            _ => PageOrientation.Portrait,
        };

        PdfPage pp = new()
        {
            Orientation = orientation,
            Width = page.Configuration.Size.Width,
            Height = page.Configuration.Size.Height
        };

        pdfDocument.AddPage(pp);

        XGraphics graphics = XGraphics.FromPdfPage(pp);
        page.Render(graphics, options);
    }

    private static void Render(this PageLayout page, XGraphics graphics, RenderOptions options) =>
        page.Content.Render(graphics, options);

    private static void Render(this IEnumerable<Layout> layouts, XGraphics graphics, RenderOptions options)
    {
        foreach (Layout layout in layouts)
        {
            layout.Render(graphics, options);
        }
    }

    private static void Render(this Layout layout, XGraphics graphics, RenderOptions options)
    {
        switch (layout)
        {
            case ElementLayout element:
                element.RenderText(graphics, options);
                break;
            case IComposedLayout composedLayout:
                composedLayout.InnerLayouts.Render(graphics, options);
                break;
        }

        layout.RenderBorder(graphics);
        layout.RenderSpecialCharacter(graphics, options);
        layout.RenderDebuggingBorder(graphics, options);
    }
}
