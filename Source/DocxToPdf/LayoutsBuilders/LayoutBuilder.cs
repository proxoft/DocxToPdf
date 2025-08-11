using System.Collections.Generic;
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

    public PageLayout[] CreatePages(DocumentModel document)
    {
        if(document.Sections.Length == 0)
        {
            return [];
        }

        List<PageLayout> pages = [];
        PageLayout currentPage = document.Sections[0].Properties.PageConfiguration.CreateNewPage(0);
        Rectangle remainingDrawingArea = currentPage.DrawingArea;
        SectionLayoutingResult layoutingResult = SectionLayoutingResult.None;

        foreach (Section section in document.Sections)
        {
            // if section needs new page, create new page
            do
            {
                layoutingResult = section.Process(
                    layoutingResult,
                    remainingDrawingArea,
                    new FieldVariables(currentPage.PageNumber, pages.Count + 1),
                    _layoutServices
                );

                currentPage = currentPage with
                {
                    Content = [.. currentPage.Content, .. layoutingResult.Layouts]
                };

                remainingDrawingArea = layoutingResult.RemainingDrawingArea;
                if (layoutingResult.Status
                    is ResultStatus.RequestDrawingArea
                    or ResultStatus.NewPageRequired)
                {
                    pages.Add(currentPage);
                    currentPage = section.Properties.PageConfiguration.CreateNewPage(currentPage.PageNumber);
                    remainingDrawingArea = currentPage.DrawingArea;
                }
            } while (layoutingResult.Status != ResultStatus.Finished);
        }

        pages.Add(currentPage);
        return [..pages];
    }
}

file static class Functions
{
    public static PageLayout CreateNewPage(this PageConfiguration pageConfiguration, int currentPageNumber)
    {
        Rectangle boundingBox = pageConfiguration.CalculatePageBoundingBox();
        Rectangle drawingRegion = pageConfiguration.CalculatePageDrawingArea();

        PageLayout page = new(currentPageNumber + 1, boundingBox, drawingRegion, [], pageConfiguration, Borders.None);
        return page;
    }

    public static Rectangle CalculatePageBoundingBox(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size);

    public static Rectangle CalculatePageDrawingArea(this PageConfiguration pageConfiguration) =>
        Rectangle.FromSize(pageConfiguration.Size)
            .CropFromLeft(pageConfiguration.Margin.Left)
            .CropFromTop(pageConfiguration.Margin.Top)
            .CropFromRight(pageConfiguration.Margin.Right)
            .CropFromBottom(pageConfiguration.Margin.Bottom);
}
