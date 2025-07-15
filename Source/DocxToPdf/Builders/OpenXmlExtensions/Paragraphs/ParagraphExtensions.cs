namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;

internal static class ParagraphExtensions
{
    public static Word.SectionProperties? GetSectionProperties(this Word.Paragraph paragraph) =>
        paragraph.ParagraphProperties?.SectionProperties;
}
