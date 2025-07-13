using System.Linq;
using Proxoft.DocxToPdf.Core.Images;
using Proxoft.DocxToPdf.Models.Styles.Services;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Builders;

internal static class ParagraphFactory
{
    public static Paragraph Create(Word.Paragraph paragraph, IImageAccessor imageAccessor, IStyleFactory styleFactory)
    {
        IStyleFactory paragraphStyleFactory = styleFactory.ForParagraph(paragraph.ParagraphProperties);
        Elements.Drawings.FixedDrawing[] fixedDrawings = [.. paragraph
            .CreateFixedDrawingElements(imageAccessor)
            .OrderBy(d => d.OffsetFromParent.Y)
        ];

        System.Collections.Generic.IEnumerable<Elements.LineElement> elements = paragraph
            .CreateParagraphElements(imageAccessor, paragraphStyleFactory);

        return new Paragraph(elements, fixedDrawings, paragraphStyleFactory);
    }
}
