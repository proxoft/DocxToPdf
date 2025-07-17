using Proxoft.DocxToPdf.Documents.Styles.Borders;

namespace Proxoft.DocxToPdf.LayoutsRendering;

internal class RenderOptions
{
    public BorderStyle SectionBorder { get; set; } = BorderStyle.None;

    public BorderStyle SectionColumnBorder { get; set; } = BorderStyle.None;

    public BorderStyle ParagraphBorder { get; set; } = BorderStyle.None;

    public BorderStyle LineBorder { get; set; } = BorderStyle.None;

    public BorderStyle WordBorder { get; set; } = BorderStyle.None;

    public bool RenderParagraphCharacter { get; set; } = false;

    public bool RenderWhitespaceCharacters { get; set; } = false;

    public bool RenderCellCharacter { get; set; } = false;
}
