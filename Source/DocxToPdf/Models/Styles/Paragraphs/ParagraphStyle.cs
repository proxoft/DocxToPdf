using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Models.Styles.Paragraphs;

internal class ParagraphStyle
{
    public static readonly ParagraphStyle Default = new(LineAlignment.Left, ParagraphSpacing.Default);

    public ParagraphStyle(
        LineAlignment lineAlignment,
        ParagraphSpacing spacing)
    {
        this.LineAlignment = lineAlignment;
        this.Spacing = spacing;
    }

    public LineAlignment LineAlignment { get; }

    public ParagraphSpacing Spacing { get; }

    public ParagraphStyle Override(
        ParagraphProperties paragraphProperties,
        IReadOnlyCollection<StyleParagraphProperties> styleParagraphs)
    {
        if(paragraphProperties is null && styleParagraphs.Count == 0)
        {
            return this;
        }

        LineAlignment lineAlignment =paragraphProperties?.Justification.GetLinesAlignment(this.LineAlignment) ?? this.LineAlignment;

        SpacingBetweenLines?[] temp = [
            paragraphProperties?.SpacingBetweenLines,
            ..styleParagraphs.Select(sp => sp.SpacingBetweenLines)
        ];

        SpacingBetweenLines[] spacings = [
            ..temp
                .Where(sbl => sbl is not null)
                .Select(sbl => sbl!)
        ];

        ParagraphSpacing spacing = this.Spacing.Override(
            spacings
        );

        return new ParagraphStyle(lineAlignment, spacing);
    }

    public static ParagraphStyle From(ParagraphPropertiesBaseStyle style)
    {
        var spacing = style?.SpacingBetweenLines?.ToParagraphSpacing(ParagraphSpacing.Default) ?? ParagraphSpacing.Default;
        var lineAlignment = style?.Justification?.GetLinesAlignment(LineAlignment.Left) ?? LineAlignment.Left;

        return new ParagraphStyle(lineAlignment, spacing);
    }
}
