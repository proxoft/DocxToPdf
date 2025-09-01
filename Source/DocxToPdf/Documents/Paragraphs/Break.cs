using DocumentFormat.OpenXml.Office2010.Excel;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs;

// internal record PageBreak(ModelId Id, TextStyle TextStyle) : Element(Id, TextStyle);

internal enum BreakType
{
    // None,
    Column,
    // Section,
    Page
}

internal record Break(
    ModelId Id,
    BreakType BreakType,
    TextStyle TextStyle) : Element(Id, TextStyle);