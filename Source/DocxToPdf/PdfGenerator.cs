using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Models;
using Proxoft.DocxToPdf.Rendering;

namespace Proxoft.DocxToPdf;

public class PdfGenerator
{
    static PdfGenerator()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public PdfDocument Generate(Stream docxStream, RenderingOptions? options = null)
    {
        using WordprocessingDocument docx = WordprocessingDocument.Open(docxStream, false);
        PdfDocument pdf = docx.Generate(options);
        return pdf;
    }

    public MemoryStream GenerateAsStream(Stream docxStream, RenderingOptions? options = null)
    {
        PdfDocument pdf = this.Generate(docxStream, options);
        MemoryStream ms = new();
        pdf.Save(ms);
        return ms;
    }

    public byte[] GenerateAsByteArray(Stream docxStream, RenderingOptions? options = null)
    {
        using MemoryStream ms = this.GenerateAsStream(docxStream, options);
        return ms.ToArray();
    }
}

file static class Operators
{
    public static PdfDocument Generate(this WordprocessingDocument docx, RenderingOptions? options = null)
    {
        RenderingOptions renderingOptions = options ?? RenderingOptions.Default;

        PdfDocument pdfDocument = new();
        PdfRenderer renderer = new(pdfDocument, renderingOptions);
        Document document = new(docx);
        document.Render(renderer);
        return pdfDocument;
    }
}
