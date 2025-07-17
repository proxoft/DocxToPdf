using System.Collections.Generic;
using System.Text;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
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
            case ParagraphLayout paragraph:
                paragraph.Lines.Render(graphics, options);
                paragraph.RenderBorder(graphics, options.ParagraphBorder);
                break;
            case LineLayout line:
                line.Childs.Render(graphics, options);
                line.RenderBorder(graphics, options.LineBorder);
                break;
            case TextLayout text:
                graphics.DrawString(text.Text.Content, _font, XBrushes.Black, new XPoint(text.BoundingBox.X, text.BoundingBox.Y));
                text.RenderBorder(graphics, options.WordBorder);
                break;
        }
    }

    private static void RenderBorder(this Layout layout, XGraphics graphics, BorderStyle borderStyle)
    {
        if(borderStyle == BorderStyle.None)
        {
            return;
        }

        XPen pen = new(XColor.FromKnownColor(XKnownColor.OrangeRed), borderStyle.Width);
        XRect rect = new(
            new XPoint(layout.BoundingBox.X, layout.BoundingBox.Y),
            new XSize(layout.BoundingBox.Width, layout.BoundingBox.Height)
        );

        graphics.DrawRectangle(pen, rect);
    }
}
