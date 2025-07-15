using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Builders.Sections;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.Builders;

internal static class DocumentBuilder
{
    public static DocumentModel CreateDocumentModel(this WordprocessingDocument docx)
    {
        if (docx.MainDocumentPart?.Document.Body is null)
        {
            return DocumentModel.Null;
        }

        BuilderServices builderServices = new();
        Section[] sections = docx.MainDocumentPart.Document.Body.ToSections(builderServices);
        return new DocumentModel(sections);
    }
}
