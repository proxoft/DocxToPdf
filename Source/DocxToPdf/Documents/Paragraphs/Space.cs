using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Space(ModelId Id, TextStyle TextStyle): Element(Id, TextStyle);
