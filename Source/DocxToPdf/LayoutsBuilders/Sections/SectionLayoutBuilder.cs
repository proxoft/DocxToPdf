using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;
using Proxoft.DocxToPdf.LayoutsBuilders.Sections;
using Proxoft.DocxToPdf.LayoutsBuilders.Tables;
using Proxoft.DocxToPdf.Layouts.Sections;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

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
        Rectangle remainingArea = drawingPageArea.MoveTo(Position.Zero);
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
                layouts = [.. layouts, .. lr.Layouts.OfType<IIdLayout>().Where(l => l.IsNotEmpty).Cast<Layout>()];
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

        Rectangle remArea = drawingPageArea
            .CropFromTop(boudingBox.Height);

        return new SectionLayoutingResult(
            section.Id,
            new SectionLayout(section.Id, layouts, boudingBox, Borders.None, partition),
            lastModelResult,
            remArea,
            resultStatus
        );
    }

    public static SectionLayoutingResult UpdateLayout(
        this SectionLayout sectionLayout,
        Section section,
        Rectangle drawingPageArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Rectangle remainingArea = drawingPageArea.MoveTo(Position.Zero);
        Layout[] updatedLayouts = [];
        LayoutingResult layoutingResult = NoLayoutingResult.Create(remainingArea);

        foreach (Layout layout in sectionLayout.Layouts)
        {
            switch(layout)
            {
                case ParagraphLayout paragraphLayout:
                    Paragraph paragraph = section.Elements
                        .OfType<Paragraph>()
                        .Single(e => e.Id == paragraphLayout.ModelId);
                    ParagraphLayoutingResult plr = paragraphLayout.Update(paragraph, remainingArea, fieldVariables, services);
                    updatedLayouts = [.. updatedLayouts, plr.ParagraphLayout];
                    remainingArea = remainingArea.CropFromTop(plr.ParagraphLayout.BoundingBox.Height);
                    break;
                // case TableLayout tableLayout:
                    // tableLayout.UpdateLayout(section, drawingPageArea, fieldVariables, services);
                    // break;
            }
        }

        Rectangle bb = updatedLayouts
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(Rectangle.Empty)
            .CalculateBoundingBox()
            .MoveTo(drawingPageArea.TopLeft);

        SectionLayout sl = new(
            section.Id,
            updatedLayouts,
            bb,
            Borders.None,
            LayoutPartition.StartEnd
        );

        return new SectionLayoutingResult(section.Id, sl, layoutingResult, remainingArea, ResultStatus.Finished);
    }
}
