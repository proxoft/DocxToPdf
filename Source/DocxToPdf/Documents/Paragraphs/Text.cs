using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Text(
    ModelId Id,
    string Content,
    TextStyle TextStyle
) : Element(Id, TextStyle);
