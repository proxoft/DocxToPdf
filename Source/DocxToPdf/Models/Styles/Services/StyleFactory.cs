using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Styles.Paragraphs;

namespace Proxoft.DocxToPdf.Models.Styles.Services;

internal class StyleFactory : IStyleFactory
{
    private readonly MainDocumentPart? _mainDocumentPart;

    private StyleFactory(
        MainDocumentPart? mainDocumentPart,
        TextStyle textStyle,
        ParagraphStyle paragraphStyle)
    {
        _mainDocumentPart = mainDocumentPart;
        this.TextStyle = textStyle;
        this.ParagraphStyle = paragraphStyle;
    }

    public TextStyle TextStyle { get; }

    public ParagraphStyle ParagraphStyle { get; }

    public static StyleFactory Default(MainDocumentPart? mainDocumentPart)
    {
        DocDefaults? docDefaults = mainDocumentPart?.StyleDefinitionsPart?.Styles?.DocDefaults;
        ParagraphStyle paragraph = ParagraphStyle.From(docDefaults?.ParagraphPropertiesDefault?.ParagraphPropertiesBaseStyle);
        TextStyle textStyle = docDefaults?.RunPropertiesDefault?.CreateTextStyle(mainDocumentPart?.ThemePart?.Theme) ?? TextStyle.Default();

        return new StyleFactory(mainDocumentPart, textStyle, paragraph);
    }

    public IStyleFactory ForParagraph(ParagraphProperties? paragraphProperties)
    {
        ParagraphStyle ps = this.EffectiveStyle(paragraphProperties);
        TextStyle ts = this.FontFromParagraph(paragraphProperties);

        return new StyleFactory(_mainDocumentPart, ts, ps);
    }

    public IStyleFactory ForTable(TableProperties tableProperties)
    {
        StyleParagraphProperties[] paragraphStyles = [..this.GetParagraphStyles(tableProperties?.TableStyle?.Val)];
        StyleRunProperties[] runStyles = [..this.GetRunStyles(tableProperties?.TableStyle?.Val)];

        ParagraphStyle ps = this.ParagraphStyle.Override(null, paragraphStyles);
        TextStyle ts = this.TextStyle.Override(null, runStyles);
        return new StyleFactory(_mainDocumentPart, ts, ps);
    }

    private ParagraphStyle EffectiveStyle(ParagraphProperties? paragraphProperties)
    {
        StyleParagraphProperties[] styles = [.. this.GetParagraphStyles(paragraphProperties?.ParagraphStyleId?.Val)];
        return this.ParagraphStyle.Override(paragraphProperties, styles);
    }

    public TextStyle EffectiveTextStyle(RunProperties? runProperties)
    {
        StyleRunProperties[] styleRuns = [.. this.GetRunStyles(runProperties?.RunStyle?.Val)];
        return this.TextStyle.Override(runProperties, styleRuns);
    }

    private TextStyle FontFromParagraph(ParagraphProperties? paragraphProperties)
    {
        StyleRunProperties[] styles = [.. this.GetRunStyles(paragraphProperties)];
        return this.TextStyle.Override(null, styles);
    }

    private IEnumerable<StyleParagraphProperties> GetParagraphStyles(StringValue? firstStyleId)
    {
        if(string.IsNullOrWhiteSpace(firstStyleId?.Value))
        {
            yield break;
        }

        StringValue? styleId = firstStyleId;
        do
        {
            Style? style = this.FindStyle(styleId);
            if (style?.StyleParagraphProperties != null)
            {
                yield return style.StyleParagraphProperties;
            }

            styleId = style?.BasedOn?.Val;
        } while (styleId != null);
    }

    private IEnumerable<StyleRunProperties> GetRunStyles(ParagraphProperties? paragraphProperties) =>
        this.FindStyles(paragraphProperties?.ParagraphStyleId?.Val);

    private IEnumerable<StyleRunProperties> GetRunStyles(StringValue? firstStyleId) =>
        this.FindStyles(firstStyleId);

    private IEnumerable<StyleRunProperties> FindStyles(StringValue? fromStyleId)
    {
        if (string.IsNullOrWhiteSpace(fromStyleId?.Value))
        {
            yield break;
        }

        StringValue? styleId = fromStyleId;
        do
        {
            Style? style = this.FindStyle(styleId);
            if (style?.StyleRunProperties != null)
            {
                yield return style.StyleRunProperties;
            }

            styleId = style?.BasedOn?.Val;
        } while (styleId != null);
    }

    private Style? FindStyle(StringValue styleId) =>
        _mainDocumentPart?
            .StyleDefinitionsPart?
            .Styles?
            .OfType<Style>()
            .SingleOrDefault(s => s.StyleId == styleId);
}
