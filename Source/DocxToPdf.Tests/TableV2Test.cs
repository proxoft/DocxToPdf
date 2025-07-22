using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsRendering;
using Proxoft.DocxToPdf.Tests.Tools;

namespace Proxoft.DocxToPdf.Tests;

public class TableV2Test
{
    private readonly RenderOptions _options = new()
    {
    };

    [Fact]
    public void Table()
    {
        PageLayout[] pages = "Tables/Table.docx".ReadAndLayoutDocument();

        pages
            .Should()
            .NotBeEmpty();

        "Tables/Table_v2.pdf".RenderAndSave(pages, _options);
    }

    [Fact]
    public void CellBorders()
    {
        PageLayout[] pages = "Tables/CellBorders.docx".ReadAndLayoutDocument();

        pages
            .Should()
            .NotBeEmpty();

        "Tables/CellBorders_v2.pdf".RenderAndSave(pages, _options);
    }

    [Fact]
    public void Layout()
    {
        PageLayout[] pages = "Tables/Layout.docx".ReadAndLayoutDocument();

        pages
            .Should()
            .NotBeEmpty();

        "Tables/Layout_v2.pdf".RenderAndSave(pages, _options);
    }

    [Fact]
    public void Layout2()
    {
        PageLayout[] pages = "Tables/Layout2.docx".ReadAndLayoutDocument();

        pages
            .Should()
            .NotBeEmpty();

        "Tables/Layout2_v2.pdf".RenderAndSave(pages, _options);
    }
}
