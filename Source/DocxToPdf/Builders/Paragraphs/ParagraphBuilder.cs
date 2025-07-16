using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;
using Proxoft.DocxToPdf.Documents.Paragraphs;

namespace Proxoft.DocxToPdf.Builders.Paragraphs;

internal static class ParagraphBuilder
{
    public static Paragraph ToParagraph(this Word.Paragraph paragraph, BuilderServices services)
    {
        Element[] elements = [
             ..paragraph
                .SelectRuns()
                .SelectMany(run => run.SplitToElements(services))
        ];

        return new Paragraph(services.IdFactory.NextParagraphId(), elements);
    }

    private static Element[] SplitToElements(this Word.Run run, BuilderServices services) =>
       [
            ..run
                .ChildElements
                .Where(c => c is Word.Text || c is Word.TabChar || c is Word.Drawing || c is Word.Break || c is Word.CarriageReturn)
                .SelectMany(c => {
                    return c switch
                    {
                        Word.Text t => t.SplitToElements(services),
                        Word.TabChar t => [new Tab(services.IdFactory.NextWordId())],
                        // Word.Drawing d => d.CreateInlineDrawing(imageAccessor),
                        // Word.CarriageReturn _ => [new NewLineElement(textStyle)],
                        // Word.Break b => b.CreateBreakElement(textStyle),
                        _ => [new Text(services.IdFactory.NextWordId(), "!ignored!", TextExtensions._default)]
                    };
                })
        ];
}
