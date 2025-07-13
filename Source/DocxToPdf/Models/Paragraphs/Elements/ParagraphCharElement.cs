using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements;

internal class ParagraphCharElement(TextStyle textStyle) : TextElement(string.Empty, _hiddenText, textStyle)
{
    private const string _hiddenText = "¶";

    public static ParagraphCharElement Create(TextStyle textStyle) =>
        new(textStyle);
}
