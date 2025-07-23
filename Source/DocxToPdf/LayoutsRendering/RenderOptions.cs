using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsRendering;

internal class RenderOptions
{
    public static readonly RenderOptions Default = new();

    public BorderStyle SectionBorder { get; set; } = BorderStyle.None;

    public BorderStyle SectionColumnBorder { get; set; } = BorderStyle.None;

    public BorderStyle ParagraphBorder { get; set; } = BorderStyle.None;

    public BorderStyle LineBorder { get; set; } = BorderStyle.None;

    public BorderStyle WordBorder { get; set; } = BorderStyle.None;

    public bool RenderParagraphCharacter { get; set; } = false;

    public bool RenderWhitespaceCharacters { get; set; } = false;

    public bool RenderCellCharacter { get; set; } = false;
}

internal static class Operators
{
    public static BorderStyle GetBorderStyle(this RenderOptions options, Layout forLayout) =>
        forLayout switch
        {
            ParagraphLayout => options.ParagraphBorder,
            LineLayout => options.LineBorder,
            TextLayout => options.WordBorder,
            SpaceLayout => options.WordBorder,
            _ => BorderStyle.None,
        };
}
