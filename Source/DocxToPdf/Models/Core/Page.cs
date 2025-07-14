using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Pages;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Core;

internal class Page(
    PageNumber pageNumber,
    PageConfiguration configuration) : IPage
{
    public static readonly Page None = new(
        PageNumber.None,
        new PageConfiguration(Size.Zero, PageOrientation.Portrait));

    public PageNumber PageNumber { get; } = pageNumber;
    public PageConfiguration Configuration { get; } = configuration;

    public PageMargin Margin { get; private set; } = PageMargin.PageNone;

    public DocumentVariables DocumentVariables { get; set; } = new DocumentVariables(0);

    public Rectangle GetContentRegion()
    {
        return new Rectangle(
            this.Margin.Left,
            this.Margin.Top,
            this.Configuration.Width - this.Margin.HorizontalMargins,
            this.Configuration.Height - this.Margin.VerticalMargins);
    }

    public Rectangle GetPageRegion() =>
        new(0, 0, this.Configuration.Width, this.Configuration.Height);

    public void SetBottomMargins(double footer, double bottom)
    {
        this.Margin = this.Margin.WithBottom(footer, bottom);
    }

    public void SetHorizontalMargins(double left, double right)
    {
        this.Margin = this.Margin.WithHorizontal(left, right);
    }

    public void SetTopMargins(double header, double top)
    {
        this.Margin = this.Margin.WithTop(header, top);
    }
}
