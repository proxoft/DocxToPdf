using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;

namespace Proxoft.DocxToPdf.Builders.Styles;

internal class StyleFactory
{
    private readonly StyleDefinitionsPart? _styleDefinitionsPart;
    private readonly ParagraphStyle _paragraph;

    private StyleFactory(StyleDefinitionsPart? styleDefinitionsPart, ParagraphStyle paragraph)
    {
        _styleDefinitionsPart = styleDefinitionsPart;
        _paragraph = paragraph;
    }

    public static StyleFactory Create(MainDocumentPart mainDocumentPart)
    {
        DocDefaults? docDefaults = mainDocumentPart?.StyleDefinitionsPart?.Styles?.DocDefaults;
        ParagraphStyle paragraph = docDefaults.CreateDefaultParagraphStyle();

        return new(mainDocumentPart?.StyleDefinitionsPart, paragraph);
    }

    public ParagraphStyle ForParagraph(ParagraphProperties? paragraphProperties)
    {
        StyleParagraphProperties[] styles = [.. _styleDefinitionsPart.GetParagraphStyles(paragraphProperties?.ParagraphStyleId?.Val)];
        return _paragraph.CreateParagraphStyle(paragraphProperties, styles);
    }
}