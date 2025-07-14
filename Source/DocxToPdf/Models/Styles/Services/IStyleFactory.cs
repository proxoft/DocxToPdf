using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Styles.Paragraphs;

namespace Proxoft.DocxToPdf.Models.Styles.Services;

internal interface IStyleFactory
{
    ParagraphStyle ParagraphStyle { get; }

    TextStyle TextStyle { get; }

    IStyleFactory ForParagraph(Word.ParagraphProperties? paragraphProperties);

    IStyleFactory ForTable(Word.TableProperties tableProperties);

    TextStyle EffectiveTextStyle(Word.RunProperties? runProperties);
}
