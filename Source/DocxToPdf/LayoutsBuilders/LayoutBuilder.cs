using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal class LayoutBuilder
{
    private LayoutServices _layoutServices = new();

    public PageLayout[] CreatePages(DocumentModel document)
    {
        if(document.Sections.Length == 0)
        {
            return [];
        }

        List<PageLayout> pages = [];
        PageLayout currentPage = document.Sections[0].Properties.PageConfiguration.CreatePage();
        Rectangle remainingDrawingArea = currentPage.DrawingArea;

        foreach(Section section in document.Sections)
        {
            // if section needs new page, create new page
            LayoutingResult result = section.Process(ModelReference.None, remainingDrawingArea, _layoutServices);

            currentPage = currentPage with
            {
                Content = [.. currentPage.Content, .. result.Layouts]
            };

            remainingDrawingArea = result.RemainingDrawingArea;
            if (!result.IsFinished)
            {
                pages.Add(currentPage);
                currentPage = section.Properties.PageConfiguration.CreatePage();
                remainingDrawingArea = currentPage.DrawingArea;
            }
        }

        pages.Add(currentPage);
        return [..pages];
    }
}

file static class Functions
{
    public static PageLayout CreatePage(this PageConfiguration pageConfiguration)
    {
        Rectangle boundingBox = pageConfiguration.CalculatePageBoundingBox();
        Rectangle drawingRegion = pageConfiguration.CalculatePageDrawingArea();

        PageLayout page = new(ModelReference.None, boundingBox, drawingRegion, [], pageConfiguration);
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
