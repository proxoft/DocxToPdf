using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs.Fields;

internal record EmptyField(ModelId Id, TextStyle TextStyle) : Field(Id, TextStyle);
