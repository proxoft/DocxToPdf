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

DocxToPdf is a fun project, and offers only limited features. If you need professional solution, consider using a different nuget.
Anyway, it will be nice to get a feedback about missing/non working features, we will try to implement/fix them.

Supported features:
- text styling, justification, alignment
- paragaphs
- tables
    - merging of cells
- page numbers (page and total pages)
- breaks: Page, Column, Section
- headers and footers
- images
- layout