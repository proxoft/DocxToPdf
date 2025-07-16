namespace Proxoft.DocxToPdf.Documents.Sections;

internal record SectionProperties(
    PageConfiguration PageConfiguration,
    ColumnConfig[] Columns
);