using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Documents.Paragraphs;

internal record Tab(ModelId Id, TextStyle TextStyle) : Element(Id, TextStyle);
