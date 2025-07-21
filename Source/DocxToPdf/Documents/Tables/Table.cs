using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Documents.Tables;

internal record Table(
    ModelId Id,
    Cell[] Cells,
    Grid Grid,
    CellBorderPattern CellBorderPattern,
    Borders Borders
) : Model(Id);
