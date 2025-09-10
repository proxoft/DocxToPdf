namespace Proxoft.DocxToPdf.Documents.Styles.Paragraphs;

internal record ParagraphSpacing(
    LineSpacing LineSpacing,
    float Before,
    float After
)
{
    public static readonly ParagraphSpacing Default = new(AutoLineSpacing.Default, 0, 10);
}

