using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Sections;

internal static class SectionLayoutBuilder
{
    public static SectionLayoutingResult Process(
            this Section section,
            SectionLayoutingResult previousLayoutingResult,
            Rectangle drawingPageArea,
            LayoutServices services)
    {
        Model[] unprocessed = section
                .Elements
                .SkipProcessed(previousLayoutingResult.LastModelLayoutingResult);

        Layout[] layouts = [];
        Rectangle remainingArea = drawingPageArea;
        LayoutingResult lastModelResult = previousLayoutingResult.LastModelLayoutingResult;
        ResultStatus resultStatus = ResultStatus.Finished;

        foreach (Model model in unprocessed)
        {
            LayoutingResult lr = model switch
            {
                Paragraph paragraph => paragraph.Process(lastModelResult, remainingArea, services),
                Table table => table.Process(lastModelResult, remainingArea, services),
                _ => NoLayoutingResult.Create(remainingArea)
            };

            if(lr.Status is ResultStatus.Finished or ResultStatus.RequestDrawingArea)
            {
                remainingArea = lr.RemainingDrawingArea;
                layouts = [.. layouts, .. lr.Layouts];
                lastModelResult = lr;
            }

            if(lr.Status != ResultStatus.Finished)
            {
                resultStatus = ResultStatus.RequestDrawingArea;
                break;
            }
        }

        return new SectionLayoutingResult(
            section.Id,
            layouts,
            lastModelResult,
            remainingArea,
            resultStatus
        );
    }
}
