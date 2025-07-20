using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;


namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class CellLayoutBuilder
{
    public static LayoutingResult Process(
        this Cell cell,
        Rectangle availableArea,
        LayoutServices services)
    {
        return new LayoutingResult(
            [],
            availableArea,
            ResultStatus.Finished
        );
    }
}
