using Proxoft.DocxToPdf.Documents.Paragraphs;

namespace Proxoft.DocxToPdf.Builders.Paragraphs;

internal static class ParagraphBuilder
{
    public static Paragraph ToParagraph(this Word.Paragraph paragraph, BuilderServices services)
    {
        return new Paragraph(services.IdFactory.NextParagraphId(), []);
    }
}
