using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsBuilders;
using Proxoft.DocxToPdf.LayoutsRendering;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class ParagraphLayoutTest
{
    [Fact]
    public void Paragraph()
    {
        DocumentModel dm = "Paragraphs/Paragraph.docx".ReadDocumentModel();
        PageLayout[] pages = new LayoutBuilder().CreatePages(dm);

        pages
            .Should()
            .NotBeEmpty();

        PdfDocument pdfDocument = LayoutRenderer.CreatePdf(pages);

        "Paragraphs/Layout_Paragraph.pdf".Save(pdfDocument);
    }
}
