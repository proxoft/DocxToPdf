﻿using Proxoft.DocxToPdf.Core;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Models.Styles
{
    internal interface IStyleFactory
    {
        // IStyleAccessor ForTableCell();
        // IStyleAccessor ForSection();

        ParagraphStyle ParagraphStyle { get; }
        TextStyle TextStyle { get; }

        IStyleFactory ForParagraph(Word.ParagraphProperties paragraphProperties);
        IStyleFactory ForTable(Word.TableProperties tableProperties);

        TextStyle EffectiveTextStyle(Word.RunProperties runProperties);
    }
}
