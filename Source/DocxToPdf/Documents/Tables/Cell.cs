using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Documents.Tables;

internal record Cell(
    ModelId Id,
    GridPosition GridPosition,
    Model[] ParagraphsOrTables,
    Padding Padding,
    Borders Borders
) : Model(Id);
