using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using Drawing = DocumentFormat.OpenXml.Drawing;

namespace Proxoft.DocxToPdf.Builders.Styles;

internal class StyleFactory
{
    private readonly StyleDefinitionsPart? _styleDefinitionsPart;
    private readonly ParagraphStyle _paragraph;

    private StyleFactory(
        StyleDefinitionsPart? styleDefinitionsPart,
        ParagraphStyle paragraph)
    {
        _styleDefinitionsPart = styleDefinitionsPart;
        _paragraph = paragraph;
    }

    public static StyleFactory Create(MainDocumentPart mainDocumentPart)
    {
        DocDefaults? docDefaults = mainDocumentPart?.StyleDefinitionsPart?.Styles?.DocDefaults;
        Drawing.Theme? theme = mainDocumentPart?.ThemePart?.Theme;

        ParagraphStyle paragraph = docDefaults.CreateDefaultParagraphStyle(theme);
        // TextStyle textStyle = docDefaults.CreateDefaultTextStyle(theme);

        return new(mainDocumentPart?.StyleDefinitionsPart, paragraph);
    }

    public ParagraphStyle ForParagraph(ParagraphProperties? paragraphProperties)
    {
        StyleParagraphProperties[] styles = [.. _styleDefinitionsPart.GetParagraphStyles(paragraphProperties?.ParagraphStyleId?.Val)];
        StyleRunProperties[] runStyles = [.._styleDefinitionsPart.GetRunStyles(paragraphProperties?.ParagraphStyleId?.Val)];
        return _paragraph.CreateParagraphStyle(paragraphProperties, styles, runStyles);
    }

    public TextStyle ForRun(RunProperties? runProperties, TextStyle paragraphTextStyle)
    {
        StyleRunProperties[] styles = [.._styleDefinitionsPart.GetRunStyles(runProperties?.RunStyle?.Val)];
        TextStyle textStyle = paragraphTextStyle.CreateTextStyle(runProperties, styles);
        return textStyle;
    }

    public StyleFactory ForTable(TableProperties tableProperties)
    {
        StyleParagraphProperties[] paragraphStyles = [.._styleDefinitionsPart.GetParagraphStyles(tableProperties?.TableStyle?.Val)];
        StyleRunProperties[] runStyles = [.._styleDefinitionsPart.GetRunStyles(tableProperties?.TableStyle?.Val)];

        ParagraphStyle ps = _paragraph.CreateParagraphStyle(null, paragraphStyles, runStyles);
        return new(_styleDefinitionsPart, ps);
    }
}