using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal abstract record Element(ModelId Id, TextStyle TextStyle): Model(Id);
