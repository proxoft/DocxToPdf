using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs.Fields;

internal abstract record Field(ModelId Id, TextStyle TextStyle) : Element(Id, TextStyle);
