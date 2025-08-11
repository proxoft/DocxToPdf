using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using D = System.Drawing;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Common;

internal class LayoutServices
{
    // result should contain additional info like: scalable for images
    public (Size boundingBox, float baseLineOffset) CalculateBoundingSizeAndBaseline(Element element)
    {
        (Size boundingBox, float baseLineOffset) = element switch
        {
            Text t => XUnitCalculator.CalculateBoundingBox(t.Content, element.TextStyle),
            Space => XUnitCalculator.CalculateBoundingBox(" ", element.TextStyle),
            Tab => XUnitCalculator.CalculateBoundingBox('\t'.ToString(), element.TextStyle),
            _ => (Size.Zero, 0)
        };

        return (boundingBox, baseLineOffset);
    }

    public float CalculateLineHeight(TextStyle textStyle)
    {
        Size s = XUnitCalculator.CalculateBoundingBox("A", textStyle).boundingBox;
        return s.Height;
    }
}

file static class XUnitCalculator
{
    private static readonly D.Graphics _graphics;
    private static readonly D.StringFormat _stringFormat;

    static XUnitCalculator()
    {
        D.Bitmap b = new(1, 1);
        _graphics = D.Graphics.FromImage(b);
        _graphics.PageUnit = D.GraphicsUnit.Point;

        _stringFormat = D.StringFormat.GenericTypographic;
        _stringFormat.Alignment = D.StringAlignment.Center;
        _stringFormat.Trimming = D.StringTrimming.None;
        _stringFormat.FormatFlags = D.StringFormatFlags.MeasureTrailingSpaces;
    }

    public static (Size boundingBox, float baseLineOffset) CalculateBoundingBox(string text, TextStyle textStyle)
    {
        D.Font font = new(textStyle.FontFamily, textStyle.FontSize, D.FontStyle.Regular);
        D.SizeF sizeF = _graphics.MeasureString(text, font, D.PointF.Empty, _stringFormat);

        float cellAscent = font.SizeInPoints * font.FontFamily.GetCellAscent(font.Style) / font.FontFamily.GetEmHeight(font.Style);
        return (new Size(sizeF.Width, sizeF.Height), sizeF.Height - cellAscent);
    }
}
