using System;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Styles.Services;
using Proxoft.DocxToPdf.Models.Tables.Builders;
using Proxoft.DocxToPdf.Models.Tables.Grids;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Images;

namespace Proxoft.DocxToPdf.Models.Tables.Elements;

internal class Cell : PageContextElement
{
    private readonly Margin _contentMargin;
    private readonly PageContextElement[] _childs = [];

    private Cell(PageContextElement[] childs, GridPosition gridPosition, BorderStyle borderStyle)
    {
        _contentMargin = new Margin(0.5, 4, 0.5, 4);
        _childs = childs;

        this.GridPosition = gridPosition;
        this.BorderStyle = borderStyle;
    }

    public GridPosition GridPosition { get; }

    public BorderStyle BorderStyle { get; }

    public override void Prepare(PageContext pageContext, Func<PagePosition, PageContextElement, PageContext> nextPageContextFactory)
    {
        var currentPageContext = pageContext
               .Crop(_contentMargin.Top, _contentMargin.Right, 0, _contentMargin.Left);

        PageContext onNewPage(PagePosition pagePosition, PageContextElement childElement)
        {
            currentPageContext = nextPageContextFactory(pagePosition, this);
            return currentPageContext.Crop(0, _contentMargin.Right, 0, _contentMargin.Left);
        }

        Rectangle availableRegion = currentPageContext.Region;

        foreach (var child in _childs)
        {
            var context = new PageContext(currentPageContext.PagePosition, availableRegion, currentPageContext.PageVariables);
            child.Prepare(context, onNewPage);

            var lastPage = child.LastPageRegion.Region;

            availableRegion = currentPageContext
                .Region
                .Clip(lastPage.BottomLeft);
        }

        this.ResetPageRegionsFrom(_childs, _contentMargin);
    }

    public override void Render(IRenderer renderer)
    {
        _childs.Render(renderer);
    }

    public static Cell From(
        Word.TableCell wordCell,
        GridPosition gridPosition,
        IImageAccessor imageAccessor,
        IStyleFactory styleFactory)
    {
        PageContextElement[] childs = [.. wordCell
            .RenderableChildren()
            .CreatePageElements(imageAccessor, styleFactory)];

        BorderStyle borderStyle = wordCell.GetBorderStyle();

        return new Cell(childs, gridPosition, borderStyle);
    }

    public override void SetPageOffset(Point pageOffset)
    {
        foreach(var child in _childs)
        {
            child.SetPageOffset(pageOffset);
        }
    }
}
