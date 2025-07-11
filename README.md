# DocxToPdf

DocxToPdf is an API for conversion Docx file into Pdf file. The library depends only on .NET and some other purely .NET packages, i.e. there's no dependency on any external tool.

```cs
string docxFilePath = "C:/Temp/Sample.docx";
string pdfOutputFilePath = "C:/Temp/Sample.pdf";

using FileStream docxStream = File.Open(docxFilePath, FileMode.Open, FileAccess.Read);
PdfGenerator pdfGenerator = new();
byte[] pdf = pdfGenerator.GenerateAsByteArray(docxStream);
File.WriteAllBytes(pdfOutputFilePath, pdf);

```
