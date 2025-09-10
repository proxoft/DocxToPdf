using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;
using Proxoft.DocxToPdf.Documents.Paragraphs.Fields;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using D = System.Drawing;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Common;

internal interface ILayoutServices
{
    public (Size boundingBox, float baseLineOffset) CalculateBoundingSizeAndBaseline(Element element, FieldVariables fieldVariables);

    public float CalculateLineHeight(TextStyle textStyle);
}

internal static class LayoutServicesFactory
{
    public static ILayoutServices CreateServices() =>
        new XUnitCalculator();
}

file class XUnitCalculator : ILayoutServices
{
    private readonly D.Graphics _graphics;
    private readonly D.StringFormat _stringFormat;
    private readonly object _lock = new();

    public XUnitCalculator()
    {
        D.Bitmap b = new(1, 1);
        _graphics = D.Graphics.FromImage(b);
        _graphics.PageUnit = D.GraphicsUnit.Point;

        _stringFormat = D.StringFormat.GenericTypographic;
        _stringFormat.Alignment = D.StringAlignment.Center;
        _stringFormat.Trimming = D.StringTrimming.None;
        _stringFormat.FormatFlags = D.StringFormatFlags.MeasureTrailingSpaces;
    }

    public (Size boundingBox, float baseLineOffset) CalculateBoundingSizeAndBaseline(Element element, FieldVariables fieldVariables)
    {
        if (element is InlineDrawing inlineDrawing)
        {
            (_, float blo) = this.CalculateBoundingBox("", element.TextStyle);
            return (inlineDrawing.Size, blo);
        }

        (Size boundingBox, float baseLineOffset) = element switch
        {
            Text t => this.CalculateBoundingBox(t.Content, element.TextStyle),
            Space => this.CalculateBoundingBox(" ", element.TextStyle),
            Tab => this.CalculateBoundingBox('\t'.ToString(), element.TextStyle),
            PageNumberField => this.CalculateBoundingBox(fieldVariables.CurrentPage.ToString(), element.TextStyle),
            TotalPagesField => this.CalculateBoundingBox(fieldVariables.TotalPages.ToString(), element.TextStyle),
            _ => (Size.Zero, 0)
        };

        return (boundingBox, baseLineOffset);
    }

    public float CalculateLineHeight(TextStyle textStyle)
    {
        Size s = this.CalculateBoundingBox("A", textStyle).boundingBox;
        return s.Height;
    }

    private (Size boundingBox, float baseLineOffset) CalculateBoundingBox(string text, TextStyle textStyle)
    {
        D.Font font = new(textStyle.FontFamily, textStyle.FontSize, D.FontStyle.Regular);
        D.SizeF sizeF = D.SizeF.Empty;
        lock (_lock)
        {
            sizeF = _graphics.MeasureString(text, font, D.PointF.Empty, _stringFormat);
        }

        float cellAscent = font.SizeInPoints * font.FontFamily.GetCellAscent(font.Style) / font.FontFamily.GetEmHeight(font.Style);
        return (new Size(sizeF.Width, sizeF.Height), sizeF.Height - cellAscent);
    }
}
