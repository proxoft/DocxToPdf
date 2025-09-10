using System.Linq;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class ParagraphAssert
{
    public static ParagraphLayout TextShouldBeEmpty(this ParagraphLayout paragraph)
    {
        foreach(LineLayout line in paragraph.Lines)
        {
            line.TextShouldBeEmpty();
        }

        return paragraph;
    }

    public static ParagraphLayout TextShouldStartAndEnd(this ParagraphLayout paragraph, string startText, string endText) =>
        paragraph
            .TextShouldStart(startText)
            .TextShouldEnd(endText);

    public static ParagraphLayout TextShouldStart(this ParagraphLayout paragraph, string text)
    {
        paragraph.Lines
            .Should()
            .NotBeEmpty();

        paragraph.Lines[0]
            .TextShouldStart(text);

        return paragraph;
    }

    public static ParagraphLayout TextShouldEnd(this ParagraphLayout paragraph, string text)
    {
        paragraph.Lines
            .Should()
            .NotBeEmpty();

        paragraph.Lines[^1]
            .TextShouldEnd(text);

        return paragraph;
    }

    public static LineLayout TextShouldBeEmpty(this LineLayout line)
    {
        line.GetText()
            .Should()
            .BeEmpty();

        return line;
    }

    public static LineLayout TextShouldStart(this LineLayout line, string text)
    {
        string lineText = line.GetText();
        lineText.Should().StartWith(text);
        return line;
    }

    public static LineLayout TextShouldEnd(this LineLayout line, string text)
    {
        string lineText = line.GetText();
        lineText.Should().EndWith(text);
        return line;
    }
}

file static class Helpers
{
    public static string GetText(this LineLayout line) =>
        string.Concat(line.Words.Select(w => w.GetText()));

    public static string GetText(this ElementLayout element) =>
        element switch
        {
            TextLayout t => t.Text.Content,
            SpaceLayout => " ",
            FieldLayout p => p.Content,
            _ => "",
        };
}
