using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

using D = System.Drawing;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal class LayoutServices
{
    private readonly TextStyle _default = new("Arial", 11, FontDecoration.Regular, new Color("000000"), Color.Empty);

    // result should contain additional info like: scalable for images
    public Size CalculateBoundingBox(Element element)
    {
        Size bb = element switch
        {
            Text t => XUnitCalculator.CalculateBoundingBox(t.Content, _default),
            Space => XUnitCalculator.CalculateBoundingBox(" ", _default),
            Tab => XUnitCalculator.CalculateBoundingBox('\t'.ToString(), _default),
            _ => Size.Zero
        };

        return bb;
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

    public static Size CalculateBoundingBox(string text, TextStyle textStyle)
    {
        D.Font font = new(textStyle.FontFamily, textStyle.FontSize, D.FontStyle.Regular);
        D.SizeF sizeF = _graphics.MeasureString(text, font, D.PointF.Empty, _stringFormat);
        return new Size(sizeF.Width, sizeF.Height);
    }
}
