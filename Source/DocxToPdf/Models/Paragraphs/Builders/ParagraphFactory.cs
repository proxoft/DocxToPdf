﻿using System.Linq;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Styles;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Builders
{
    internal static class ParagraphFactory
    {
        public static Paragraph Create(Word.Paragraph paragraph, IImageAccessor imageAccessor, IStyleFactory styleFactory)
        {
            var paragraphStyleFactory = styleFactory.ForParagraph(paragraph.ParagraphProperties);
            var fixedDrawings = paragraph
                .CreateFixedDrawingElements(imageAccessor)
                .OrderBy(d => d.OffsetFromParent.Y)
                .ToArray();

            var elements = paragraph
                .CreateParagraphElements(imageAccessor, paragraphStyleFactory);

            return new Paragraph(elements, fixedDrawings, paragraphStyleFactory);
        }
    }
}
