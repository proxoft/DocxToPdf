using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal class LayoutBuilder
{
    private readonly LayoutServices _layoutServices = new();

    public PageLayout[] CreatePages(DocumentModel document) =>
        document.Sections.Length == 0
            ? []
            : this.CreatePages(document.Sections);

    private PageLayout[] CreatePages(Section[] sections)
    {
        List<PageLayout> pages = [];
        PageLayout currentPage = sections[0].CreateNewPage(0);
        Rectangle remainingDrawingArea = currentPage.DrawingArea;
        SectionLayoutingResult layoutingResult = SectionLayoutingResult.None;

        bool done = false;
        int minimalTotalPages = 1;

        while (!done)
        {
            Section section = sections
                .SkipProcessed(layoutingResult)
                .First();

            FieldVariables fieldVariables = new(currentPage.PageNumber, Math.Max(minimalTotalPages, pages.Count + 1));
            layoutingResult = section.Process(
                    layoutingResult,
                    remainingDrawingArea,
                    fieldVariables,
                    _layoutServices
                );

            currentPage = currentPage with
            {
                Content = [.. currentPage.Content, ..layoutingResult.Layouts]
            };

            remainingDrawingArea = layoutingResult.RemainingDrawingArea;

            if (layoutingResult.Status
                    is ResultStatus.RequestDrawingArea
                    or ResultStatus.NewPageRequired)
            {
                minimalTotalPages++;
                pages.Add(currentPage);

                // try update previous pages. Possible outcomes:
                // page not changed at all
                // page updated, changes do not exceed the page
                // page update, some original layouts must be moved to next page => restart layouting

                currentPage = section.CreateNewPage(currentPage.PageNumber);
                remainingDrawingArea = currentPage.DrawingArea;
            }

            // add a safeguard for maximum iterations equal to number of elements in DocumentModel
            done = layoutingResult.Status == ResultStatus.Finished
                && layoutingResult.ModelId == sections.Last().Id;
        }

        pages.Add(currentPage);
        return [.. pages];
    }
}

file static class Functions
{
    public static PageLayout CreateNewPage(this Section section, int currentPageNumber) =>
        section.Properties.PageConfiguration.CreateNewPage(currentPageNumber);

    private static PageLayout CreateNewPage(this PageConfiguration pageConfiguration, int currentPageNumber)
    {
        Rectangle boundingBox = pageConfiguration.CalculatePageBoundingBox();
        Rectangle drawingRegion = pageConfiguration.CalculatePageDrawingArea();

        PageLayout page = new(currentPageNumber + 1, boundingBox, drawingRegion, [], pageConfiguration, Borders.None);
        return page;
    }

    private static Rectangle CalculatePageBoundingBox(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size);

    private static Rectangle CalculatePageDrawingArea(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size)
            .CropFromLeft(pageConfiguration.Margin.Left)
            .CropFromTop(pageConfiguration.Margin.Top)
            .CropFromRight(pageConfiguration.Margin.Right)
            .CropFromBottom(pageConfiguration.Margin.Bottom);
}
