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
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Documents.Shared;
using System.Linq;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Sections;

internal static class SectionLayoutBuilder
{
    public static SectionLayoutingResult Process(
            this Section section,
            SectionLayoutingResult previousLayoutingResult,
            Rectangle drawingPageArea,
            FieldVariables fieldVariables,
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
                Paragraph paragraph => paragraph.Process(lastModelResult, remainingArea, fieldVariables, services),
                Table table => table.Process(lastModelResult, remainingArea, fieldVariables, services),
                _ => NoLayoutingResult.Create(remainingArea)
            };

            if(lr.Status is ResultStatus.Finished or ResultStatus.RequestDrawingArea or ResultStatus.NewPageRequired)
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

        Rectangle boudingBox = layouts
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(new Rectangle(drawingPageArea.TopLeft, Size.Zero))
            .CalculateBoundingBox();

        LayoutPartition partition = resultStatus.CalculateLayoutPartition(previousLayoutingResult);

        return new SectionLayoutingResult(
            section.Id,
            new SectionLayout(section.Id, layouts, boudingBox, Borders.None, partition),
            lastModelResult,
            remainingArea,
            resultStatus
        );
    }
}
