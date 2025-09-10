using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Builders;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts.Pages;
using Proxoft.DocxToPdf.LayoutsBuilders;
using Proxoft.DocxToPdf.LayoutsRendering;

namespace Proxoft.DocxToPdf;

public class PdfGenerator
{
    static PdfGenerator()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public PdfDocument Generate(Stream docxStream, RenderOptions? options = null)
    {
        using WordprocessingDocument docx = WordprocessingDocument.Open(docxStream, false);
        PdfDocument pdf = docx.Generate(options ?? RenderOptions.Default);
        return pdf;
    }

    public MemoryStream GenerateAsStream(Stream docxStream, RenderOptions? options = null)
    {
        PdfDocument pdf = this.Generate(docxStream, options);
        MemoryStream ms = new();
        pdf.Save(ms);
        return ms;
    }

    public byte[] GenerateAsByteArray(Stream docxStream, RenderOptions? options = null)
    {
        using MemoryStream ms = this.GenerateAsStream(docxStream, options);
        return ms.ToArray();
    }
}

file static class Operators
{
    public static PdfDocument Generate(this WordprocessingDocument docx, RenderOptions options)
    {
        DocumentModel documentModel = docx.CreateDocumentModel();
        PageLayout[] pages = new LayoutBuilder().CreatePages(documentModel);
        PdfDocument pdfDocument = LayoutRenderer.CreatePdf(pages, options);
        return pdfDocument;
    }
}
