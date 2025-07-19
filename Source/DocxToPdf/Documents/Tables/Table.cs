namespace Proxoft.DocxToPdf.Documents.Tables;

internal record Table(
    ModelId Id,
    Cell[] Cells,
    Grid Grid,
    Borders Borders
) : Model(Id);
