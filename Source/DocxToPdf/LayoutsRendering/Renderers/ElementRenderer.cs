using PdfSharp.Drawing;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsRendering.Renderers;

internal static class ElementRenderer
{
    public static void RenderText(this ElementLayout layout, XGraphics graphics, RenderOptions renderOptions)
    {
        XFont font = layout.GetTextStyle().ToXFont();
        XBrush brush = layout.GetTextStyle().Brush.ToXBrush();

        string text = layout switch
        {
            TextLayout t => t.Text.Content,
            SpaceLayout => renderOptions.RenderWhitespaceCharacters ? "·" : " ",
            FieldLayout f => f.Content,
            _ => ""
        };

        if(layout.GetTextStyle().Background != Color.Empty)
        {
            XBrush backgroundBrush = layout.GetTextStyle().Background.ToXBrush();
            graphics.DrawRectangle(backgroundBrush, layout.BoundingBox.ToXRect());
        }

        Position position = new(layout.BoundingBox.X, layout.BoundingBox.Bottom - layout.LineBaseLineOffset);
        graphics.DrawString(text, font, brush, position.ToXPoint());
    }
}
