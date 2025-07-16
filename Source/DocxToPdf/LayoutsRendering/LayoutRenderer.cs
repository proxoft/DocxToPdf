using System.Collections.Generic;
using System.Text;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsRendering;

internal static class LayoutRenderer
{
    private static readonly XFont _font;

    static LayoutRenderer()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _font = new("Arial", 11, XFontStyle.Regular);
    }

    public static PdfDocument CreatePdf(PageLayout[] pages)
    {
        PdfDocument pdf = new();

        foreach (PageLayout page in pages)
        {
            page.CreatePdfPage(pdf);
        }

        return pdf;
    }

    private static void CreatePdfPage(this PageLayout page, PdfDocument pdfDocument)
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
        page.Render(graphics);
    }

    private static void Render(this PageLayout page, XGraphics graphics) =>
        page.Content.Render(graphics);

    private static void Render(this IEnumerable<Layout> layouts, XGraphics graphics)
    {
        foreach (Layout layout in layouts)
        {
            layout.Render(graphics);
        }
    }

    private static void Render(this Layout layout, XGraphics graphics)
    {
        switch (layout)
        {
            case ParagraphLayout paragraph:
                paragraph.Lines.Render(graphics);
                break;
            case LineLayout line:
                line.Childs.Render(graphics);
                break;
            case TextLayout text:
                graphics.DrawString(text.Text.Content, _font, XBrushes.Black, new XPoint(text.BoundingBox.X, text.BoundingBox.Y));
                break;
        }
    }
}
