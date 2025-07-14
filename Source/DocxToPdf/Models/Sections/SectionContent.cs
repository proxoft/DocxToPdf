using System;
using Proxoft.DocxToPdf.Core.Pages;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Paragraphs;
using Proxoft.DocxToPdf.Models.Sections.Columns;

namespace Proxoft.DocxToPdf.Models.Sections;

internal class SectionContent(
    PageContextElement[] childs,
    ColumnsConfiguration columnsConfiguration,
    SectionContentBreak sectionBreak) : PageElement
{
    private readonly PageContextElement[] _childs = childs;
    private readonly ColumnsConfiguration _columnsConfiguration = columnsConfiguration;

    public SectionContentBreak SectionBreak { get; } = sectionBreak;

    public void Prepare(
        PageRegion previousSection,
        PageRegion previousContent,
        SectionContentBreak previousBreak,
        Func<PageNumber, IPage> pageFactory)
    {
        PageContext context = this.CreateInitialPageContext(previousSection, previousContent, previousBreak, pageFactory);

        PageContext childContextRequest(PagePosition pagePosition, PageContextElement child) =>
            this.OnChildPageContextRequest(pagePosition, pageFactory);

        double spaceAfterPrevious = 0.0;
        foreach(PageContextElement child in _childs)
        {
            child.Prepare(context, childContextRequest);
            PageRegion lastRegion = child.LastPageRegion;
            spaceAfterPrevious = child.CalculateSpaceAfter(_childs);
            context = this.CreateContextForPagePosition(lastRegion.PagePosition, lastRegion.Region, spaceAfterPrevious, pageFactory);
        }

        this.ResetPageRegionsFrom(_childs);
    }

    public override void Render(IRenderer renderer)
    {
        _childs.Render(renderer);
        this.RenderBorders(renderer, renderer.Options.SectionBorders);
    }

    private PageContext CreateInitialPageContext(
        PageRegion previousSection,
        PageRegion previousContent,
        SectionContentBreak previousBreak,
        Func<PageNumber, IPage> pageFactory)
    {
        int spaceAfterPrevious = 0;
        switch (previousBreak)
        {
            case SectionContentBreak.None:
                {
                    PagePosition pp = previousSection.PagePosition.SamePage(PageColumn.First, _columnsConfiguration.ColumnsCount);
                    return this.CreateContextForPagePosition(pp, previousSection.Region, spaceAfterPrevious, pageFactory);
                }
            case SectionContentBreak.Column:
                {
                    PagePosition pp = previousContent.PagePosition.Next();
                    Rectangle occupiedRegion = pp.PageNumber == previousSection.PagePosition.PageNumber
                        ? previousSection.Region
                        : Rectangle.Empty;

                    return this.CreateContextForPagePosition(pp, occupiedRegion, spaceAfterPrevious, pageFactory);
                }
            case SectionContentBreak.Page:
                {
                    PagePosition pp = previousContent.PagePosition.NextPage(PageColumn.First, _columnsConfiguration.ColumnsCount);
                    return this.CreateContextForPagePosition(pp, Rectangle.Empty, spaceAfterPrevious, pageFactory);
                }
            default:
                throw new RendererException("unhandled section break;");
        }
    }

    private PageContext OnChildPageContextRequest(
        PagePosition pagePosition,
        Func<PageNumber, IPage> pageFactory)
    {
        PagePosition nextPosition = pagePosition.Next();
        PageContext context = this.CreateContextForPagePosition(nextPosition, Rectangle.Empty, 0, pageFactory);
        return context;
    }

    private PageContext CreateContextForPagePosition(
        PagePosition pagePosition,
        Rectangle occupiedRegion,
        double spaceAfterPrevious,
        Func<PageNumber, IPage> pageFactory)
    {
        IPage page = pageFactory(pagePosition.PageNumber);
        HorizontalSpace columnSpace = _columnsConfiguration.CalculateColumnSpace(pagePosition.PageColumnIndex);
        Rectangle region = page
            .GetContentRegion()
            .CropHorizontal(columnSpace.X, columnSpace.Width);

        PageContext context = new(
            pagePosition,
            region,
            page.DocumentVariables);

        double cropTop = occupiedRegion.BottomY == 0
            ? spaceAfterPrevious
            : occupiedRegion.BottomY + spaceAfterPrevious - page.Margin.Top;

        // TODO: check -0.001
        context = context.CropFromTop(Math.Min(cropTop, context.Region.Height - 0.001));
        return context;
    }
}
