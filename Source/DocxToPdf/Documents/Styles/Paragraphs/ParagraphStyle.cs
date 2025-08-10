using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Styles.Paragraphs;

internal record ParagraphStyle(
    LineAlignment LineAlignment,
    ParagraphSpacing ParagraphSpacing,
    TextStyle TextStyle)
{
    public static readonly ParagraphStyle Default = new(LineAlignment.Left, ParagraphSpacing.Default, TextStyle.Default);
}