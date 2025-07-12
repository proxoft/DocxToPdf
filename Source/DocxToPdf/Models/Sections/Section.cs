using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Pages;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Footers;
using Proxoft.DocxToPdf.Models.Footers.Builders;
using Proxoft.DocxToPdf.Models.Headers;
using Proxoft.DocxToPdf.Models.Headers.Builders;
using Proxoft.DocxToPdf.Models.Styles.Services;

namespace Proxoft.DocxToPdf.Models.Sections;

internal class Section : PageElement
{
    private List<Page> _pages = [];
    private Dictionary<PageNumber, HeaderBase> _headers = [];
    private Dictionary<PageNumber, FooterBase> _footers = [];

    private readonly SectionProperties _properties;
    private readonly IImageAccessor _imageAccessor;
    private readonly SectionContent[] _contents;
    private readonly IStyleFactory _styleFactory;

    public Section(
        SectionProperties properties,
        IEnumerable<SectionContent> sectionContents,
        IImageAccessor imageAccessor,
        IStyleFactory styleFactory)
    {
        _properties = properties;
        _imageAccessor = imageAccessor;
        _contents = sectionContents.ToArray();
        _styleFactory = styleFactory;
    }

    public IReadOnlyCollection<IPage> Pages => _pages;
    public HeaderFooterConfiguration HeaderFooterConfiguration => _properties.HeaderFooterConfiguration;

    public void Prepare(
        PageRegion previousSection,
        PageMargin previousSectionMargin,
        DocumentVariables documentVariables)
    {
        var sectionBreak = _properties.StartOnNextPage
            ? SectionContentBreak.Page
            : SectionContentBreak.None;

        IPage contentPageRequest(PageNumber pageNumber) =>
            this.OnPageRequest(pageNumber, previousSection.PagePosition.PageNumber, previousSectionMargin, documentVariables);

        var contentLastPosition = previousSection;
        foreach (var content in _contents)
        {
            content.Prepare(previousSection, contentLastPosition, sectionBreak, contentPageRequest);
            contentLastPosition = content.LastPageRegion;
            sectionBreak = content.SectionBreak;
        }

        var pr = _contents
            .SelectMany(c => c.PageRegions)
            .UnionThroughColumns()
            .ToArray();
        this.ResetPageRegions(pr);
    }

    public override void Render(IRenderer renderer)
    {
        //// page content borders
        //foreach (var p in _pages)
        //{
        //    var r = p.GetContentRegion();
        //    var rp = renderer.Get(p.PageNumber);
        //    var pen = new System.Drawing.Pen(System.Drawing.Color.Orange, 0.5f);

        //    rp.RenderLine(r.TopLine(pen));
        //    rp.RenderLine(r.RightLine(pen));
        //    rp.RenderLine(r.BottomLine(pen));
        //    rp.RenderLine(r.LeftLine(pen));
        //}

        foreach(var header in _headers.Values)
        {
            header.Render(renderer);
        }

        foreach (var footer in _footers.Values)
        {
            footer.Render(renderer);
        }

        foreach (var content in _contents)
        {
            content.Render(renderer);
        }

        // this.RenderBordersIf(renderer, renderer.Options.SectionRegionBoundaries);
    }

    private IPage OnPageRequest(
        PageNumber pageNumber,
        PageNumber previousSectionLastPage,
        PageMargin previousSectionMargin,
        DocumentVariables documentVariables)
    {
        var page = _pages.SingleOrDefault(p => p.PageNumber == pageNumber);
        if (page == null)
        {
            page = new Page(pageNumber, _properties.PageConfiguration);
            page.SetHorizontalMargins(_properties.Margin.Left, _properties.Margin.Right);
            _pages.Add(page);
        }

        page.DocumentVariables = documentVariables;

        this.CreateOrUpdateHeader(page, previousSectionLastPage, previousSectionMargin);
        var header = _headers[pageNumber];
        page.SetTopMargins(header.TopY, header.BottomY);

        this.CreateOrUpdateFooter(page, previousSectionLastPage, previousSectionMargin);
        var footer = _footers[pageNumber];
        page.SetBottomMargins(footer.FooterMargin, footer.HeightWithFooterMargin);

        return page;
    }

    private void CreateOrUpdateHeader(IPage page, PageNumber previousSectionLastPage, PageMargin previousSectionMargin)
    {
        if (!_headers.ContainsKey(page.PageNumber))
        {
            var header = previousSectionLastPage == page.PageNumber
                ? HeaderFactory.CreateInheritedHeader(previousSectionMargin)
                : _properties.HeaderFooterConfiguration
                    .FindHeader(page.PageNumber)
                    .CreateHeader(_properties.Margin, _imageAccessor, _styleFactory);

            _headers.Add(page.PageNumber, header);
        }

        _headers[page.PageNumber].Prepare(page);
    }

    private void CreateOrUpdateFooter(IPage page, PageNumber previousSectionLastPage, PageMargin previousSectionMargin)
    {
        if (!_footers.ContainsKey(page.PageNumber))
        {
            var footer = previousSectionLastPage == page.PageNumber
                ? FooterFactory.CreateInheritedFooter(previousSectionMargin)
                : _properties.HeaderFooterConfiguration
                     .FindFooter(page.PageNumber)
                     .CreateFooter(_properties.Margin, _imageAccessor, _styleFactory);

            _footers.Add(page.PageNumber, footer);
        }

        _footers[page.PageNumber].Prepare(page);
    }
}
