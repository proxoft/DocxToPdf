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
        PageLayout[] pages = [];
        PageLayout currentPage = sections[0].CreateNewPage();
        Rectangle remainingDrawingArea = currentPage.DrawingArea;
        SectionLayoutingResult lastSectionLayoutingResult = SectionLayoutingResult.None;

        bool done = false;
        int minimalTotalPages = 1;
        int currentPageNumber = 1;

        while (!done)
        {
            Section section = sections
                .SkipProcessed(lastSectionLayoutingResult)
                .First();

            FieldVariables fieldVariables = new(currentPageNumber, Math.Max(minimalTotalPages, minimalTotalPages));
            lastSectionLayoutingResult = section.Process(
                    lastSectionLayoutingResult,
                    remainingDrawingArea,
                    fieldVariables,
                    _layoutServices
                );

            currentPage = currentPage with
            {
                Content = [.. currentPage.Content, lastSectionLayoutingResult.SectionLayout]
            };

            remainingDrawingArea = lastSectionLayoutingResult.RemainingDrawingArea;

            if (lastSectionLayoutingResult.Status
                    is ResultStatus.RequestDrawingArea
                    or ResultStatus.NewPageRequired)
            {
                minimalTotalPages++;
                pages = [..pages, currentPage];

                //(pages, lastSectionLayoutingResult) = pages.Update(
                //    minimalTotalPages,
                //    sections,
                //    lastSectionLayoutingResult,
                //    _layoutServices
                //);

                //section = sections
                //    .SkipProcessed(lastSectionLayoutingResult)
                //    .First();

                currentPage = section.CreateNewPage();
                currentPageNumber++;
                remainingDrawingArea = currentPage.DrawingArea;
            }

            // add a safeguard for maximum iterations equal to number of elements in DocumentModel
            done = lastSectionLayoutingResult.Status == ResultStatus.Finished
                && lastSectionLayoutingResult.ModelId == sections.Last().Id;
        }

        pages = [..pages, currentPage];
        return pages;
    }
}

file static class Functions
{
    public static PageLayout CreateNewPage(this Section section) =>
        section.Properties.PageConfiguration.CreateNewPage();

    private static PageLayout CreateNewPage(this PageConfiguration pageConfiguration)
    {
        Rectangle boundingBox = pageConfiguration.CalculatePageBoundingBox();
        Rectangle drawingRegion = pageConfiguration.CalculatePageDrawingArea();

        PageLayout page = new(boundingBox, drawingRegion, [], pageConfiguration, Borders.None);
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
