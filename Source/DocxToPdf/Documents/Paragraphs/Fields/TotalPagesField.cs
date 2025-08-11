using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs.Fields;

internal record TotalPagesField(ModelId Id, TextStyle TextStyle): Field(Id, TextStyle);
