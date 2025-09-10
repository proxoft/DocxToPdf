using System.Linq;
using System.Collections.Generic;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Paragraphs;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Documents.Paragraphs.Fields;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;

namespace Proxoft.DocxToPdf.Builders.Paragraphs;

internal static class ParagraphBuilder
{
    public static Paragraph ToParagraph(this Word.Paragraph paragraph, BuilderServices services)
    {
        ParagraphStyle paragraphStyle = services.Styles.ForParagraph(paragraph.ParagraphProperties);

        Element[] elements = [
            ..paragraph
                .ChunkRunsByFieldType()
                .SelectMany(chunk => chunk.SplitToElements(services, paragraphStyle))
        ];

        FixedDrawing[] fixedDrawings = paragraph
            .SelectRuns()
            .CreateFixedDrawings(services);

        return new Paragraph(
            services.IdFactory.NextParagraphId(),
            [..elements],
            fixedDrawings,
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
            Word.Drawing d when d.IsInlineDrawing() => d.CreateInlineDrawing(textStyle, services),
            Word.Drawing => [], // fixed drawings are processed separately
            // Word.CarriageReturn _ => [new NewLineElement(textStyle)],
            // Word.Break => [new PageBreak(services.IdFactory.NextWordId(), textStyle.ResizeFont(-2))],
            Word.Break b => [..b.ToBreak(textStyle.ResizeFont(-2), services.IdFactory)],
            _ => [new Text(services.IdFactory.NextWordId(), "!ignored!", textStyle)]
        };

    private static FixedDrawing[] CreateFixedDrawings(this IEnumerable<Word.Run> runs, BuilderServices services) =>
        [..runs.SelectMany(r => r.CreateFixedDrawings(services))];

    private static IEnumerable<FixedDrawing> CreateFixedDrawings(this Word.Run run, BuilderServices services) =>
        run.ChildElements
            .OfType<Word.Drawing>()
            .Where(e => e.IsFixedDrawing())
            .SelectMany(d => d.CreateFixedDrawing(services));

    private static IEnumerable<RunChunk> ChunkRunsByFieldType(this Word.Paragraph paragraph)
    {
        Stack<Word.Run> runs = paragraph.SelectRuns().ToStackReversed();
        while(runs.Count > 0)
        {
            bool isField = runs.Peek().IsFieldStart();
            Word.Run[] chunk = isField
                ? [..runs.ReadFieldRuns()]
                : [..runs.ReadNonFieldRuns()];

            yield return new RunChunk(chunk, isField);
        }
    }

    private static IEnumerable<Element> SplitToElements(this RunChunk runChunk, BuilderServices services, ParagraphStyle textStyle) =>
        runChunk.IsField
            ? runChunk.Runs.CreateField(services, textStyle)
            : runChunk.Runs.SelectMany(r => r.SplitToElements(services, textStyle));

    private static IEnumerable<Element> CreateField(this Word.Run[] runs, BuilderServices services, ParagraphStyle paragraphStyle)
    {
        if(runs.Length <= 1)
        {
            yield break;
        }

        Word.Run run = runs[1];

        TextStyle textStyle = services.Styles.ForRun(run.RunProperties, paragraphStyle.TextStyle);
        Word.FieldCode fieldCode = run
            .ChildsOfType<Word.FieldCode>()
            .Single();

        Field field = fieldCode.Text.CreateField(services.IdFactory.NextWordId(), textStyle);
        yield return field;
    }

    private static IEnumerable<Word.Run> ReadNonFieldRuns(this Stack<Word.Run> stack)
    {
        while(stack.Count > 0)
        {
            if (stack.Peek().IsFieldStart()) yield break;
            yield return stack.Pop();
        }
    }

    private static IEnumerable<Word.Run> ReadFieldRuns(this Stack<Word.Run> stack)
    {
        while (stack.Count > 0)
        {
            Word.Run run = stack.Pop();
            yield return run;
            if(run.IsFieldEnd()) yield break;
        }
    }

    private static IEnumerable<Break> ToBreak(this Word.Break @break, TextStyle textStyle, ModelIdFactory idFactory)
    {
        if (@break.Type is null) yield break;
        if (@break.Type == Word.BreakValues.Column) yield return new Break(idFactory.NextWordId(), BreakType.Column, textStyle);
        if (@break.Type == Word.BreakValues.Page) yield return new Break(idFactory.NextWordId(), BreakType.Page, textStyle);
    }

    private record RunChunk(Word.Run[] Runs, bool IsField);

    private static Field CreateField(this string text, ModelId modelId, TextStyle textStyle)
    {
        string[] items = text.Split("\\");
        if (items.Length == 0)
        {
            return new EmptyField(modelId, textStyle);
        }

        return items[0].Trim() switch
        {
            "PAGE" => new PageNumberField(modelId, textStyle),
            "NUMPAGES" => new TotalPagesField(modelId, textStyle),
            _ => new EmptyField(modelId, textStyle),
        };
    }
}