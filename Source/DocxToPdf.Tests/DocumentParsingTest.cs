using Proxoft.DocxToPdf.Builders;
using DocumentFormat.OpenXml.Packaging;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Tests;

public class DocumentParsingTest
{
    private const string _samples = "../../../../../Repository/Source/Samples/";

    [Fact]
    public void ParseSimpleParagraph()
    {
        using WordprocessingDocument docx = WordprocessingDocument.Open($"{_samples}/Paragraphs/Paragraph.docx", isEditable: false);
        DocumentModel dm = docx.CreateDocumentModel();

        dm.Sections
            .Should()
            .NotBeEmpty();
    }
}
