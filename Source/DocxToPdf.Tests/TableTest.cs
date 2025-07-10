namespace Proxoft.DocxToPdf.Tests;

public class TableTest : TestBase
{
    public TableTest() : base("Tables")
    {
        this.Options = RenderingOptions.WithDefaults();
    }

    [Fact]
    public void Table()
    {
        this.Generate(nameof(Table));
    }

    [Fact]
    public void TableBorders()
    {
        this.Generate(nameof(TableBorders));
    }

    [Fact]
    public void CellBorders()
    {
        this.Generate(nameof(CellBorders));
    }

    [Fact]
    public void Layout()
    {
        this.Generate(nameof(Layout));
    }

    [Fact]
    public void TableWithParagraphsSM()
    {
        this.Generate(nameof(TableWithParagraphsSM));
    }

    [Fact]
    public void TableWithParagraphsXL()
    {
        this.Generate(nameof(TableWithParagraphsXL));
    }

    [Fact]
    public void TableWithParagraphsXXL()
    {
        this.Generate(nameof(TableWithParagraphsXXL));
    }

    [Fact]
    public void TableWithParagraphsXXXL()
    {
        this.Generate(nameof(TableWithParagraphsXXXL));
    }

    [Fact]
    public void TableWithTable()
    {
        this.Generate(nameof(TableWithTable));
    }

    [Fact]
    public void TableInSectionColumns()
    {
        this.Generate(nameof(TableInSectionColumns));
    }

    [Fact]
    public void TableOverSectionColumns()
    {
        this.Generate(nameof(TableOverSectionColumns));
    }

    [Fact]
    public void TableOverSectionColumnsOverPages()
    {
        this.Generate(nameof(TableOverSectionColumnsOverPages));
    }

    [Fact(Skip = "not implemented")]
    public void TableAlignment()
    {
        this.Generate(nameof(TableAlignment));
    }
}
