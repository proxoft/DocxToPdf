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
        bool previousWasFinished = previousLayoutingResult.LastModelLayoutingResult.Status == ResultStatus.Finished;
        Model[] unprocessed = [
            ..section
                .Elements
                .SkipProcessed(previousLayoutingResult.LastModelLayoutingResult.ModelId, previousWasFinished)
        ];

        Layout[] layouts = [];
        LayoutingResult lastModelResult = NoLayoutingResult.Instance;
        ResultStatus resultStatus = ResultStatus.Finished;
        Rectangle remainingArea = drawingPageArea;

        foreach (Model model in unprocessed)
        {
            LayoutingResult lr = model switch
            {
                Paragraph paragraph => paragraph.ProcessParagraph(previousLayoutingResult.LastModelLayoutingResult, remainingArea, services),
                Table table => table.ProcessTable(previousLayoutingResult.LastModelLayoutingResult, remainingArea, services),
                _ => NoLayoutingResult.Instance
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

    private static ParagraphLayoutingResult ProcessParagraph(this Paragraph paragraph, LayoutingResult previousResult, Rectangle remainingArea, LayoutServices services)
    {
        ParagraphLayoutingResult plr = previousResult.AsResultOfModel(paragraph.Id, ParagraphLayoutingResult.None);
        return paragraph.Process(plr, remainingArea, services);
    }

    private static TableLayoutingResult ProcessTable(this Table table, LayoutingResult previousResult, Rectangle remainingArea, LayoutServices services)
    {
        TableLayoutingResult tlr = previousResult.AsResultOfModel(table.Id, TableLayoutingResult.None);
        return table.Process(tlr, remainingArea, services);
    }
}
