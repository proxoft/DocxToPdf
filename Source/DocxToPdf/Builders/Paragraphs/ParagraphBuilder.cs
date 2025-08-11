using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Builders.Paragraphs;

internal static class ParagraphBuilder
{
    public static Paragraph ToParagraph(this Word.Paragraph paragraph, BuilderServices services)
    {
        ParagraphStyle paragraphStyle = services.Styles.ForParagraph(paragraph.ParagraphProperties);

        Element[] elements = [
             ..paragraph
                .SelectRuns()
                .SelectMany(run => run.SplitToElements(services, paragraphStyle))
        ];

        return new Paragraph(
            services.IdFactory.NextParagraphId(),
            elements,
            paragraphStyle
        );
    }

    private static Element[] SplitToElements(this Word.Run run, BuilderServices services, ParagraphStyle paragraphStyle)
    {
        TextStyle textStyle = services.Styles.ForRun(run.RunProperties, paragraphStyle.TextStyle);

        return [
             ..run
                 .ChildElements
                 .Where(c => c is Word.Text || c is Word.TabChar || c is Word.Drawing || c is Word.Break || c is Word.CarriageReturn)
                 .SelectMany(c => c.Split(services, textStyle))
        ] ;
    }

    private static Element[] Split(this OpenXml.OpenXmlElement e, BuilderServices services, TextStyle textStyle) =>
        e switch
        {
            Word.Text t => t.SplitToElements(services, textStyle),
            Word.TabChar => [new Tab(services.IdFactory.NextWordId(), textStyle)],
            // Word.Drawing d => d.CreateInlineDrawing(imageAccessor),
            // Word.CarriageReturn _ => [new NewLineElement(textStyle)],
            Word.Break => [new PageBreak(services.IdFactory.NextWordId(), textStyle.ResizeFont(-2))],
            _ => [new Text(services.IdFactory.NextWordId(), "!ignored!", textStyle)]
        };
}
