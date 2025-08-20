using System.IO;
using DocumentFormat.OpenXml.Packaging;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Builders;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsBuilders;
using Proxoft.DocxToPdf.LayoutsRendering;

namespace Proxoft.DocxToPdf.Tests.Tools;

internal static class DocumentModelReader
{
    private const string _samples = "../../../../../Repository/Source/Samples/";
    private const string _outputFolder = $"../../../../TestOutputs/";

    public static PageLayout[] ReadAndLayoutDocument(this string docxSubpath)
    {
        DocumentModel dm = docxSubpath.ReadDocumentModel();
        PageLayout[] pages = new LayoutBuilder().CreatePages(dm);
        return pages;
    }

    public static DocumentModel ReadDocumentModel(this string docxSubpath)
    {
        using WordprocessingDocument docx = WordprocessingDocument.Open($"{_samples}/{docxSubpath}", isEditable: false);
        DocumentModel dm = docx.CreateDocumentModel();
        return dm;
    }

    public static void RenderAndSave(this string pdfSubpath, PageLayout[] pages, RenderOptions? options = null)
    {
        PdfDocument pdfDocument = LayoutRenderer.CreatePdf(pages, options ?? new RenderOptions());
        pdfSubpath.Save(pdfDocument);
    }

    public static void Save(this string pdfSubpath, PdfDocument pdfDocument)
    {
        using MemoryStream ms = new();
        pdfDocument.Save(ms);
        string filePath = $"{_outputFolder}/{pdfSubpath}";
        string directory = Path.GetDirectoryName(filePath) ?? throw new DirectoryNotFoundException("Output directory not found.");
        if(!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes($"{_outputFolder}/{pdfSubpath}", ms.ToArray());
    }
}
