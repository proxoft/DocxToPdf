using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using Drawing = DocumentFormat.OpenXml.Drawing;

namespace Proxoft.DocxToPdf.Builders.Styles;

internal static class ParagraphStyleBuilder
{
    public static ParagraphStyle CreateDefaultParagraphStyle(this DocDefaults? docDefaults, Drawing.Theme? theme)
    {
        if(docDefaults is null)
        {
            return ParagraphStyle.Default;
        }

        TextStyle textStyle = docDefaults.CreateDefaultTextStyle(theme);

        ParagraphStyle? ps = docDefaults
            .ParagraphPropertiesDefault?
            .ParagraphPropertiesBaseStyle
            .CreateDefaultParagraphStyle(textStyle);

        return ps ?? ParagraphStyle.Default;
    }

    private static ParagraphStyle CreateDefaultParagraphStyle(this ParagraphPropertiesBaseStyle? style, TextStyle textStyle)
    {
        LineAlignment lineAlignment = style?.Justification.GetLinesAlignment(LineAlignment.Left) ?? LineAlignment.Left;
        ParagraphSpacing spacing = style?.SpacingBetweenLines?.ToParagraphSpacing(ParagraphSpacing.Default) ?? ParagraphSpacing.Default;
        return new ParagraphStyle(lineAlignment, spacing, textStyle);
    }

    public static ParagraphStyle CreateParagraphStyle(
        this ParagraphStyle defaultStyle,
        ParagraphProperties? paragraphProperties,
        StyleParagraphProperties[] styles,
        StyleRunProperties[] runStyles)
    {
        Justification?[] justifications = [
            paragraphProperties?.Justification,
            ..styles
            .Select(s => s.Justification)
        ];

        LineAlignment lineAlignment = justifications
            .Where(j => j is not null)
            .FirstOrDefault()
            .GetLinesAlignment(defaultStyle.LineAlignment);

        ParagraphSpacing paragraphSpacing = defaultStyle.ParagraphSpacing.CreateParagraphSpacing(paragraphProperties, styles);
        TextStyle textStyle = defaultStyle.TextStyle.CreateTextStyle(null, runStyles);

        return new ParagraphStyle(lineAlignment, paragraphSpacing, textStyle);
    }

    private static ParagraphSpacing ToParagraphSpacing(
        this SpacingBetweenLines? spacingXml,
        ParagraphSpacing ifNull)
    {
        if (spacingXml == null)
        {
            return ifNull;
        }

        float before = spacingXml.Before.ToPoint();
        float after = spacingXml.After.ToPoint();
        LineSpacing line = spacingXml.GetLineSpacing();

        return new ParagraphSpacing(line, before, after);
    }

    public static LineAlignment GetLinesAlignment(
        this Justification? justification,
        LineAlignment ifNull)
    {
        if (justification?.Val is null)
        {
            return ifNull;
        }

        if (justification.Val.Value == JustificationValues.Left) return LineAlignment.Left;
        if (justification.Val.Value == JustificationValues.Center) return LineAlignment.Center;
        if (justification.Val.Value == JustificationValues.Right) return LineAlignment.Right;
        if (justification.Val.Value == JustificationValues.Both) return LineAlignment.Justify;

        return LineAlignment.Left;
    }

    private static ParagraphSpacing CreateParagraphSpacing(
        this ParagraphSpacing defaultSpacing,
        ParagraphProperties? paragraphProperties,
        IEnumerable<StyleParagraphProperties> styles)
    {
        SpacingBetweenLines?[] spacingBetweenLines = [
            paragraphProperties?.SpacingBetweenLines, // ordering is important
            ..styles.Select(s => s.SpacingBetweenLines)
        ];

        float before = spacingBetweenLines
            .Select(s => s?.Before)
            .Where(b => b is not null)
            .FirstOrDefault()?.ToPoint() ?? defaultSpacing.Before;

        float after = spacingBetweenLines
            .Select(s => s?.After)
            .Where(b => b is not null)
            .FirstOrDefault()?.ToPoint() ?? defaultSpacing.After;


        LineSpacing lineSpacing = defaultSpacing.LineSpacing
            .CreateLineSpacing(spacingBetweenLines);

        return new ParagraphSpacing(
            lineSpacing,
            before,
            after
        );
    }

    private static LineSpacing GetLineSpacing(this SpacingBetweenLines spacingBetweenLines)
    {
        LineSpacingRuleValues rule = spacingBetweenLines.LineRule?.Value ?? Word.LineSpacingRuleValues.Auto;
        LineSpacing lineSpacing = rule.ToLineSpacing(spacingBetweenLines.Line);
        return lineSpacing;
    }

    private static LineSpacing CreateLineSpacing(
        this LineSpacing defaultLineSpacing,
        IEnumerable<SpacingBetweenLines?> spacingBetweenLines)
    {
        StringValue? lineHeight = spacingBetweenLines
            .Select(sp => sp?.Line)
            .FirstOrDefault();

        LineSpacingRuleValues? rule = spacingBetweenLines
                .Select(sp => sp?.LineRule?.Value)
                .Where(lr => lr is not null)
                .FirstOrDefault();

        return rule?.ToLineSpacing(lineHeight) ?? defaultLineSpacing;
    }

    private static LineSpacing ToLineSpacing(this LineSpacingRuleValues rule, StringValue? line)
    {
        LineSpacing lineSpacing = AutoLineSpacing.Default;
        if (rule == LineSpacingRuleValues.Auto && line?.ToLong() is not null) lineSpacing = new AutoLineSpacing(line.ToLong());
        if (rule == LineSpacingRuleValues.Exact) lineSpacing = new ExactLineSpacing(line.ToPoint());
        if (rule == LineSpacingRuleValues.AtLeast) lineSpacing = new AtLeastLineSpacing(line.ToPoint());
        return lineSpacing;
    }
}
