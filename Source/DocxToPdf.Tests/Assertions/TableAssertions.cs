using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class TableAssertions
{
    public static CellLayout ShouldContainCell(this TableLayout table, int cellId)
    {
        ModelId modelId = new("cel", cellId);
        table.Cells
            .Any(c => c.ModelId == modelId)
            .Should()
            .BeTrue();

        return table.Cells.Single(c => c.ModelId == modelId);
    }

    
}
