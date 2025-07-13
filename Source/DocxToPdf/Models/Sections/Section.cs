using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core.Images;
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

internal class Section(
    SectionProperties properties,
    SectionContent[] sectionContents,
    IImageAccessor imageAccessor,
    IStyleFactory styleFactory) : PageElement
{
    private readonly List<Page> _pages = [];
    private readonly Dictionary<PageNumber, HeaderBase> _headers = [];
    private readonly Dictionary<PageNumber, FooterBase> _footers = [];

    private readonly SectionProperties _properties = properties;
    private readonly IImageAccessor _imageAccessor = imageAccessor;
    private readonly SectionContent[] _contents = sectionContents;
    private readonly IStyleFactory _styleFactory = styleFactory;

    public IReadOnlyCollection<IPage> Pages => _pages;

    public HeaderFooterConfiguration HeaderFooterConfiguration => _properties.HeaderFooterConfiguration;

    public void Prepare(
        PageRegion previousSection,
        PageMargin previousSectionMargin,
        DocumentVariables documentVariables)
    {
        SectionContentBreak sectionBreak = _properties.StartOnNextPage
            ? SectionContentBreak.Page
            : SectionContentBreak.None;

        IPage contentPageRequest(PageNumber pageNumber) =>
            this.OnPageRequest(pageNumber, previousSection.PagePosition.PageNumber, previousSectionMargin, documentVariables);

        PageRegion contentLastPosition = previousSection;
        foreach (var content in _contents)
        {
            content.Prepare(previousSection, contentLastPosition, sectionBreak, contentPageRequest);
            contentLastPosition = content.LastPageRegion;
            sectionBreak = content.SectionBreak;
        }

        PageRegion[] pr = [.. _contents
            .SelectMany(c => c.PageRegions)
            .UnionThroughColumns()];

        this.ResetPageRegions(pr);
    }

    public override void Render(IRenderer renderer)
    {
        foreach (HeaderBase header in _headers.Values)
        {
            header.Render(renderer);
        }

        foreach (FooterBase footer in _footers.Values)
        {
            footer.Render(renderer);
        }

        foreach (SectionContent content in _contents)
        {
            content.Render(renderer);
        }
    }

    private Page OnPageRequest(
        PageNumber pageNumber,
        PageNumber previousSectionLastPage,
        PageMargin previousSectionMargin,
        DocumentVariables documentVariables)
    {
        Page? page = _pages.SingleOrDefault(p => p.PageNumber == pageNumber);
        if (page is null)
        {
            page = new Page(pageNumber, _properties.PageConfiguration);
            page.SetHorizontalMargins(_properties.Margin.Left, _properties.Margin.Right);
            _pages.Add(page);
        }

        page.DocumentVariables = documentVariables;

        this.CreateOrUpdateHeader(page, previousSectionLastPage, previousSectionMargin);
        HeaderBase header = _headers[pageNumber];
        page.SetTopMargins(header.TopY, header.BottomY);

        this.CreateOrUpdateFooter(page, previousSectionLastPage, previousSectionMargin);
        FooterBase footer = _footers[pageNumber];
        page.SetBottomMargins(footer.FooterMargin, footer.HeightWithFooterMargin);

        return page;
    }

    private void CreateOrUpdateHeader(Page page, PageNumber previousSectionLastPage, PageMargin previousSectionMargin)
    {
        if (!_headers.ContainsKey(page.PageNumber))
        {
            HeaderBase header = previousSectionLastPage == page.PageNumber
                ? HeaderFactory.CreateInheritedHeader(previousSectionMargin)
                : _properties.HeaderFooterConfiguration
                    .FindHeader(page.PageNumber)
                    .CreateHeader(_properties.Margin, _imageAccessor, _styleFactory);

            _headers.Add(page.PageNumber, header);
        }

        _headers[page.PageNumber].Prepare(page);
    }

    private void CreateOrUpdateFooter(Page page, PageNumber previousSectionLastPage, PageMargin previousSectionMargin)
    {
        if (!_footers.ContainsKey(page.PageNumber))
        {
            FooterBase footer = previousSectionLastPage == page.PageNumber
                ? FooterFactory.CreateInheritedFooter(previousSectionMargin)
                : _properties.HeaderFooterConfiguration
                     .FindFooter(page.PageNumber)
                     .CreateFooter(_properties.Margin, _imageAccessor, _styleFactory);

            _footers.Add(page.PageNumber, footer);
        }

        _footers[page.PageNumber].Prepare(page);
    }
}
