using Proxoft.DocxToPdf.LayoutsRendering;
using Proxoft.DocxToPdf.Tests.Tools;
using Proxoft.DocxToPdf.Tests.Assertions;
using Proxoft.DocxToPdf.Layouts.Tables;
using Proxoft.DocxToPdf.Documents.Styles;
using Proxoft.DocxToPdf.Documents.Styles.Borders;

namespace Proxoft.DocxToPdf.Tests;

public class TableV2Test
{
    private readonly DocxToPdfExecutor _executor = new(
        "Tables/{0}.docx",
        "Tables/{0}_v2.pdf",
        new RenderOptions()
        {
            SectionBorder = new BorderStyle(new Color("FF0000"), 1, LineStyle.Solid),
            RenderParagraphCharacter = true
        }
    );

    [Fact]
    public void Table()
    {
        _executor.Convert("Table", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void CellBorders()
    {
        _executor.Convert("CellBorders", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void Layout()
    {
        _executor.Convert("Layout", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void Layout2()
    {
        _executor.Convert("Layout2", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void TableWithParagraphsXXL()
    {
        _executor.Convert("TableWithParagraphsXXL", pages =>
        {
            pages.CountShouldBe(2);
        });
    }

    [Fact]
    public void TableWithOneColumnParagraphsXXXL()
    {
        _executor.Convert("TableWithOneColumnParagraphsXXXL", pages =>
        {
            pages
                .CountShouldBe(3);

            TableLayout table = pages[0]
                .PageShouldContainSingleTable();

            table
                .ShouldContainCell(1)
                .CellShouldContainOneParagraph()
                .TextShouldStart("Lorem")
                .TextShouldEnd("pretium.")
                ;

            table
                .ShouldContainCell(2)
                .CellShouldContainOneParagraph()
                .TextShouldStart("Mauris")
                .TextShouldEnd(", id ")
                ;

            table = pages[1]
                .PageShouldContainSingleTable();

            table
                .ShouldContainCell(2)
                .CellShouldContainOneParagraph()
                .TextShouldStart("tincidunt")
                .TextShouldEnd("diam.")
                ;

            table
                .ShouldContainCell(3)
                .CellShouldContainOneParagraph()
                .TextShouldStart("Aliquam")
                .TextShouldEnd("suscipit.")
                ;

            table = pages[1]
                .PageShouldContainSingleTable();

            table
                .ShouldContainCell(4)
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
                .CountShouldBe(3);

            #region page 1
            TableLayout table = pages[0]
                .PageShouldContainSingleTable();

            #region cell 1
            CellLayout cell = table
                .ShouldContainCell(1)
                .ShouldBeComplete()
                ;

            cell
                .ShouldContainParagraphOnPosition(0)
                .TextShouldStart("Cell 1");

            cell
                .ShouldContainParagraphOnPosition(1)
                .TextShouldStartAndEnd("Lorem", "pretium.");

            cell
                .ShouldContainParagraphOnPosition(2)
                .TextShouldBeEmpty();

            cell
                .ShouldContainParagraphOnPosition(3)
                .TextShouldStart("End Cell 1");

            cell
               .ShouldContainParagraphOnPosition(4)
               .TextShouldBeEmpty();
            #endregion

            #region cell 2
            cell = table
                .ShouldContainCell(2)
                .ShouldBeComplete()
                .ShouldStartAndEndWithText("Cell 2", "End Cell 2");

            #endregion

            #region cell 3
            cell = table
                .ShouldContainCell(3)
                .ShouldBeComplete();

            cell
                .ShouldContainParagraphOnPosition(0)
                .TextShouldStart("Cell 3");

            cell
                .ShouldContainParagraphOnPosition(1)
                .TextShouldStartAndEnd("Quisque", "mauris.");

            cell
                .ShouldContainParagraphOnPosition(2)
                .TextShouldBeEmpty();

            cell
                .ShouldContainParagraphOnPosition(3)
                .TextShouldStart("End Cell 3");

            #endregion

            #region cell 4
            cell = table
                .ShouldContainCell(4)
                .ShouldBeStart(only: true);

            cell.ShouldContainParagraphOnPosition(0)
                .TextShouldStart("Cell 4");
            cell.ShouldContainParagraphOnPosition(1)
                .TextShouldStartAndEnd("Mauris", "consequat. ");
            #endregion

            #region cell 5
            cell = table
                .ShouldContainCell(5)
                .ShouldBeStart(only: true);

            cell.ShouldContainParagraphOnPosition(0)
                .TextShouldStart("Cell 5");
            cell.ShouldContainParagraphOnPosition(1)
                .TextShouldStartAndEnd("Vestibulum", "finibus ");
            #endregion

            #region cell 6
            cell = table
               .ShouldContainCell(6)
               .ShouldBeStart(only: true);

            cell.ShouldContainParagraphOnPosition(0)
                .TextShouldStart("Cell 6");
            cell.ShouldContainParagraphOnPosition(1)
                .TextShouldStartAndEnd("est", "mi.");
            cell.ShouldContainParagraphOnPosition(2)
                .TextShouldBeEmpty();
            cell.ShouldContainParagraphOnPosition(3)
                .TextShouldEnd("End Cell 6");
            #endregion
            #endregion

            #region page 2
            table = pages[1]
                .PageShouldContainSingleTable();

            _ = table
                .ShouldContainCell(4)
                .ShouldBeEnd(only: true)
                .ShouldStartAndEndWithText("Suspendisse", "End Cell 4");
            
            _ = table
                .ShouldContainCell(5)
                .ShouldBeEnd(only: true)
                .ShouldStartAndEndWithText("ullamcorper", "End Cell 5");

            _ = table
                .ShouldContainCell(6)
                .ShouldBeEmpty()
                .ShouldBeEnd(only: true);

            _ = table
                .ShouldContainCell(7)
                .ShouldBeComplete()
                .ShouldStartAndEndWithText("Cell 7", "End Cell 7");

            _ = table
                .ShouldContainCell(8)
                .ShouldBeComplete()
                .ShouldStartAndEndWithText("Cell 8", "End Cell 8");

            _ = table
                .ShouldContainCell(9)
                .ShouldStartAndEndWithText("Cell 9", "End Cell 9");

            _ = table
                .ShouldContainCell(10)
                .ShouldBeStart(only: true)
                .ShouldStartAndEndWithText("Cell 10", "facilisis sapien ");

            _ = table
                .ShouldContainCell(11)
                .ShouldBeStart(only: true)
                .ShouldStartAndEndWithText("Cell 11", "quam egestas ");

            _ = table
                .ShouldContainCell(12)
                .ShouldBeStart(only: true)
                .ShouldStartAndEndWithText("Cell 12", "nec libero ");
            #endregion

            #region page 3
            table = pages[2]
                .PageShouldContainSingleTable();

            _ = table
                .ShouldContainCell(10)
                .ShouldStartAndEndWithText("nulla", "End Cell 10");

            _ = table
                .ShouldContainCell(11)
                .ShouldStartAndEndWithText("euismod ", "End Cell 11");

            _ = table
                .ShouldContainCell(12)
                .ShouldStartAndEndWithText("vulputate", "End Cell 12");
            #endregion
        });
    }

    [Fact]
    public void TableWithTable()
    {
        _executor.Convert("TableWithTable", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void TableWithTableOverPage()
    {
        _executor.Convert("TableWithTableOverPage", pages =>
        {
            pages.CountShouldBe(2);
        });
    }

    [Fact]
    public void TableInSectionColumns()
    {
        _executor.Convert("TableInSectionColumns", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void TableOverSectionColumns()
    {
        _executor.Convert("TableOverSectionColumns", pages =>
        {
            pages.CountShouldBe(1);
        });
    }

    [Fact]
    public void TableOverSectionColumnsOverPages()
    {
        _executor.Convert("TableOverSectionColumnsOverPages", pages =>
        {
            pages.CountShouldBe(2);
        });
    }
}
