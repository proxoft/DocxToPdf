using System.IO;
using DocumentFormat.OpenXml.Packaging;
using PdfSharp.Pdf;
using Proxoft.DocxToPdf.Builders;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Tests.Tools;

internal static class DocumentModelReader
{
    private const string _samples = "../../../../../Repository/Source/Samples/";
    private const string _outputFolder = $"../../../../TestOutputs/";

    public static DocumentModel ReadDocumentModel(this string docxSubpath)
    {
        using WordprocessingDocument docx = WordprocessingDocument.Open($"{_samples}/{docxSubpath}", isEditable: false);
        DocumentModel dm = docx.CreateDocumentModel();
        return dm;
    }

    public static void Save(this string pdfSubpath, PdfDocument pdfDocument)
    {
        using MemoryStream ms = new();
        pdfDocument.Save(ms);
        File.WriteAllBytes($"{_outputFolder}/{pdfSubpath}", ms.ToArray());
    }
}
