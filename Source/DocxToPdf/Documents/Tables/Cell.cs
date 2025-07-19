namespace Proxoft.DocxToPdf.Documents.Tables;

internal record Cell(
    ModelId Id,
    GridPosition GridPosition,
    Model[] ParagraphsOrTables,
    Borders Borders
) : Model(Id);

internal record GridPosition(int Column, int ColumnSpan, int Row, int RowSpan);
