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

        RenderOptions options = new()
        {
            // ParagraphBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("000000"), 1, Documents.Styles.Borders.LineStyle.Solid),
            // LineBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("000000"), 1, Documents.Styles.Borders.LineStyle.Solid),
            WordBorder = new Documents.Styles.Borders.BorderStyle(new Documents.Styles.Color("000000"), 1, Documents.Styles.Borders.LineStyle.Solid)
        };

        PdfDocument pdfDocument = LayoutRenderer.CreatePdf(pages, options);

        "Paragraphs/Layout_Paragraph.pdf".Save(pdfDocument);
    }
}
