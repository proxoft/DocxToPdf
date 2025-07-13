using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core;
using WDrawing = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Styles.Services;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Extensions.Units;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements.Drawings;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Core.Images;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Builders;

internal static class ParagraphElementsBuilder
{
    public static IEnumerable<LineElement> CreateParagraphElements(
        this Word.Paragraph paragraph,
        IImageAccessor imageAccessor,
        IStyleFactory styleFactory)
    {
        Stack<Word.Run> runs = paragraph
            .SelectRuns()
            .ToStackReversed();

        List<LineElement> elements = [];

        while (runs.Count > 0)
        {
            Word.Run run = runs.Pop();
            if (run.IsFieldStart())
            {
                List<Word.Run> fieldRuns = [ run ];
                do
                {
                    run = runs.Pop();
                    fieldRuns.Add(run);
                } while (!run.IsFieldEnd());

                LineElement field = fieldRuns.CreateField(styleFactory);
                elements.Add(field);
            }
            else
            {
                LineElement[] runElements = run.CreateParagraphElements(imageAccessor, styleFactory);
                elements.AddRange(runElements);
            }
        }

        return elements.Union([ParagraphCharElement.Create(styleFactory.TextStyle)]);
    }

    public static IEnumerable<FixedDrawing> CreateFixedDrawingElements(this Word.Paragraph paragraph, IImageAccessor imageAccessor)
    {
        FixedDrawing[] drawings = [
            ..paragraph
                .Descendants<Word.Drawing>()
                .Where(d => d.Anchor != null)
                .Select(d => d.Anchor!.ToFixedDrawing(imageAccessor))
        ];

        return drawings;
    }

    private static LineElement[] CreateParagraphElements(
        this Word.Run run,
        IImageAccessor imageAccessor,
        IStyleFactory styleAccessor)
    {
        TextStyle textStyle = styleAccessor.EffectiveTextStyle(run.RunProperties);

        LineElement[] elements = [
            ..run
                .ChildElements
                .Where(c => c is Word.Text || c is Word.TabChar || c is Word.Drawing || c is Word.Break || c is Word.CarriageReturn)
                .SelectMany(c => {
                    return c switch
                    {
                        Word.Text t => t.SplitTextToElements(textStyle),
                        Word.TabChar t => [new TabElement(textStyle)],
                        Word.Drawing d => d.CreateInlineDrawing(imageAccessor),
                        Word.CarriageReturn _ => [new NewLineElement(textStyle)],
                        Word.Break b => b.CreateBreakElement(textStyle),
                        _ => throw new RendererException("unprocessed child")
                    };
                })
        ];

        return elements;
    }

    private static LineElement[] CreateBreakElement(this Word.Break breakXml, TextStyle textStyle) =>
        breakXml.Type is null
            ? [new NewLineElement(textStyle)]
            : [];

    private static LineElement[] SplitTextToElements(this Word.Text text, TextStyle textStyle) =>
        [..text.InnerText
            .SplitToWordsAndWhitechars()
            .Select(s =>
            {
                return s switch
                {
                    " " => (LineElement)new SpaceElement(textStyle),
                    _ => new WordElement(s, textStyle)
                };
            })
        ];

    private static InilineDrawing[] CreateInlineDrawing(this Word.Drawing drawing, IImageAccessor imageAccessor)
    {
        if (drawing.Inline == null)
        {
            return [];
        }

        InilineDrawing inlineDrawing = drawing.Inline.ToInilineDrawing(imageAccessor);
        return [ inlineDrawing ];
    }

    private static InilineDrawing ToInilineDrawing(this WDrawing.Inline inline, IImageAccessor imageAccessor)
    {
        Size size = inline.Extent.ToSize();
        DocumentFormat.OpenXml.Drawing.Blip blipElement = inline.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().First();

        return new InilineDrawing(blipElement.Embed?.Value ?? "", size, imageAccessor);
    }

    private static FixedDrawing ToFixedDrawing(this WDrawing.Anchor anchor, IImageAccessor imageAccessor)
    {
        Size size = anchor.Extent.ToSize();
        DocumentFormat.OpenXml.Drawing.Blip blipElement = anchor.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().First();

        Margin margin = anchor.ToAnchorMargin();
        Point position = (anchor.SimplePos?.Value ?? false)
            ? new Point(anchor.SimplePosition?.X?.Value ?? 0, anchor.SimplePosition?.Y?.Value ?? 0)
            : new Point(anchor.HorizontalPosition?.PositionOffset.ToDouble() ?? 0, anchor.VerticalPosition?.PositionOffset.ToDouble() ?? 0);

        return new FixedDrawing(blipElement.Embed?.Value ?? "", position, size, margin, imageAccessor);
    }

    private static Margin ToAnchorMargin(this WDrawing.Anchor anchor)
    {
        double top = anchor.DistanceFromTop.EmuToPoint();
        double right = anchor.DistanceFromRight.EmuToPoint();
        double bottom = anchor.DistanceFromBottom.EmuToPoint();
        double left = anchor.DistanceFromLeft.EmuToPoint();

        return new Margin(top, right, bottom, left);
    }
}
