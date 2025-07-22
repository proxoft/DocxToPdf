using Proxoft.DocxToPdf.LayoutsRendering;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class TableV2Test
{
    private DocxToPdfExecutor _executor = new(
        "Tables/{0}.docx",
        "Tables/{0}_v2.pdf",
        RenderOptions.Default
    );

    [Fact]
    public void Table()
    {
        _executor.Convert("Table", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void CellBorders()
    {
        _executor.Convert("CellBorders", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void Layout()
    {
        _executor.Convert("Layout", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void Layout2()
    {
        _executor.Convert("Layout2", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }
}
