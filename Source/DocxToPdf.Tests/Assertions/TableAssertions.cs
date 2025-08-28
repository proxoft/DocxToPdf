using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests.Assertions;

internal static class TableAssertions
{
    public static CellLayout ShouldContainCell(this TableLayout table, ModelId cellId)
    {
        table.Cells
            .Any(c => c.ModelId == cellId)
            .Should()
            .BeTrue();

        return table.Cells.Single(c => c.ModelId == cellId);
    }
}
