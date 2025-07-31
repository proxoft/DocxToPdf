namespace Proxoft.DocxToPdf.Documents.Styles.Paragraphs;

internal record ParagraphStyle(
    LineAlignment LineAlignment,
    ParagraphSpacing ParagraphSpacing)
{
    public static readonly ParagraphStyle Default = new(LineAlignment.Left, ParagraphSpacing.Default);
}