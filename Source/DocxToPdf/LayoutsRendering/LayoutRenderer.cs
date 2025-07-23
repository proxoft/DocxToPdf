using System.Collections.Generic;
using System.Text;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
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
            case IComposedLayout composedLayout:
                composedLayout.InnerLayouts.Render(graphics, options);
                break;
            case TextLayout text:
                graphics.DrawString(text.Text.Content, _font, XBrushes.Black, new XPoint(text.BoundingBox.X, text.BoundingBox.Bottom - text.BaselineOffset));
                break;
            case SpaceLayout space:
                graphics.DrawString("·", _font, XBrushes.Black, new XPoint(space.BoundingBox.X, space.BoundingBox.Bottom - space.BaselineOffset));
                break;
        }

        layout.RenderBorder(graphics);
        layout.RenderDebuggingBorder(graphics, options);
    }

    private static void RenderBorder(this Layout layout, XGraphics graphics)
    {
        if(layout.Borders == Borders.None)
        {
            return;
        }

        layout.BoundingBox.LeftLine.RenderBorder(layout.Borders.Left, graphics);
        if(layout.Partition is LayoutPartition.Start or LayoutPartition.StartEnd)
        {
            layout.BoundingBox.TopLine.RenderBorder(layout.Borders.Top, graphics);
        }
        
        layout.BoundingBox.RightLine.RenderBorder(layout.Borders.Right, graphics);
        if (layout.Partition is LayoutPartition.End or LayoutPartition.StartEnd)
        {
            layout.BoundingBox.BottomLine.RenderBorder(layout.Borders.Bottom, graphics);
        }
    }

    private static void RenderBorder(this (Position start, Position end) line, BorderStyle borderStyle, XGraphics graphics)
    {
        if(borderStyle == BorderStyle.None
            || borderStyle.Width == 0
            || borderStyle.LineStyle == LineStyle.None)
        {
            return;
        }

        XPen pen = borderStyle.ToXPen();
        graphics.DrawLine(pen, line.start.ToXPoint(), line.end.ToXPoint());
    }

    private static void RenderDebuggingBorder(this Layout layout, XGraphics graphics, RenderOptions options)
    {
        BorderStyle borderStyle = options.GetBorderStyle(layout);
        if(borderStyle == BorderStyle.None)
        {
            return;
        }

        XPen pen = borderStyle.ToXPen();
        XRect rect = layout.BoundingBox.ToXRect();

        graphics.DrawRectangle(pen, rect);
    }
}

file static class PdfSharpConversion
{
    public static XPoint ToXPoint(this Position position) =>
        new(position.X, position.Y);

    public static XPen ToXPen(this BorderStyle borderStyle)
    {
        (int r, int g, int b) = borderStyle.Color.Rgb;
        float width = borderStyle.LineStyle == LineStyle.None
            ? 0
            : borderStyle.Width;

        XColor color = borderStyle.LineStyle == LineStyle.None
            ? XColor.Empty
            : XColor.FromArgb(0, r, g, b);

        return new(color, width)
        {
            DashStyle = borderStyle.LineStyle.ToXDashStyle()
        };
    }

    public static XRect ToXRect(this Rectangle rectangle) =>
        new (
            new XPoint(rectangle.X, rectangle.Y),
            new XSize(rectangle.Width, rectangle.Height)
        );

    private static XDashStyle ToXDashStyle(this LineStyle lineStyle) =>
        lineStyle switch
        {
            LineStyle.None => XDashStyle.Solid,
            LineStyle.Solid => XDashStyle.Solid,
            LineStyle.Dashed => XDashStyle.Dash,
            LineStyle.Dotted => XDashStyle.Dot,
            LineStyle.DotDash => XDashStyle.DashDot,
            LineStyle.DotDotDash => XDashStyle.DashDotDot,
            _ => XDashStyle.Solid
        };
}
