using System.Linq;
using Proxoft.DocxToPdf.LayoutsRendering;
using Proxoft.DocxToPdf.Tests.Tools;
using Proxoft.DocxToPdf.Tests.Assertions;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.Tests;

public class TableV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
        "Tables/{0}.docx",
        "Tables/{0}_v2.pdf",
        new RenderOptions()
        {
            RenderParagraphCharacter = true
        }
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

    [Fact]
    public void TableWithParagraphsXXL()
    {
        _executor.Convert("TableWithParagraphsXXL", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void TableWithOneColumnParagraphsXXXL()
    {
        _executor.Convert("TableWithOneColumnParagraphsXXXL", pages =>
        {
            pages
                .Length
                .Should()
                .Be(3);

            TableLayout table = pages[0]
                .PageShouldContainSingleTable();

            table
                .ShouldContainCell(new Documents.ModelId("cel", 1))
                .CellShouldContainOneParagraph()
                .TextShouldStart("Lorem")
                .TextShouldEnd("pretium.")
                ;

            table
                .ShouldContainCell(new Documents.ModelId("cel", 2))
                .CellShouldContainOneParagraph()
                .TextShouldStart("Mauris")
                .TextShouldEnd(", id ")
                ;

            table = pages[1]
                .PageShouldContainSingleTable();

            table
                .ShouldContainCell(new Documents.ModelId("cel", 2))
                .CellShouldContainOneParagraph()
                .TextShouldStart("tincidunt")
                .TextShouldEnd("diam.")
                ;

            table
                .ShouldContainCell(new Documents.ModelId("cel", 3))
                .CellShouldContainOneParagraph()
                .TextShouldStart("Aliquam")
                .TextShouldEnd("suscipit.")
                ;

            table = pages[1]
                .PageShouldContainSingleTable();

            table
                .ShouldContainCell(new Documents.ModelId("cel", 4))
                .CellShouldContainOneParagraph()
                .TextShouldStart("Pellentesque")
                .TextShouldEnd("sodales ")
                ;

            pages[2]
                .ShouldContainSingleParagraph()
                .TextShouldBeEmpty()
                ;
        });
    }

    [Fact]
    public void TableWithParagraphsXXXL()
    {
        _executor.Convert("TableWithParagraphsXXXL", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void TableWithTable()
    {
        _executor.Convert("TableWithTable", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }

    [Fact]
    public void TableWithTableOverPage()
    {
        _executor.Convert("TableWithTableOverPage", pages =>
        {
            pages
                .Should()
                .NotBeEmpty();
        });
    }
}
