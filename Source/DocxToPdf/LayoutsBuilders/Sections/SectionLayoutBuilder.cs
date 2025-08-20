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
using System.Collections.Generic;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Sections;

internal static class SectionLayoutBuilder
{
    public static (SectionLayout, ProcessingInfo) CreateLayout(
        this Section section,
        SectionLayout previousSectionLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Model[] unprocessed = section.Unprocessed(previousSectionLayout.Layouts);
        Layout[] layouts = [];
        Size remainingArea = availableArea;
        ProcessingInfo sectionProcessingInfo = ProcessingInfo.Ignore;
        float yOffset = 0;
        ModelId lastProcessed = ModelId.None;

        foreach (Model model in unprocessed)
        {
            (Layout layout, ProcessingInfo processingInfo) result = model switch
            {
                Paragraph p => p.CreateLayout(previousSectionLayout.TryFindParagraphLayout(p.Id), remainingArea, fieldVariables, services),
                _ => (NoLayout.Instance, ProcessingInfo.Ignore)
            };

            if (result.processingInfo == ProcessingInfo.Ignore)
            {
                continue;
            }
            else
            {
                sectionProcessingInfo = result.processingInfo;
            }

            if(result.processingInfo is not ProcessingInfo.IgnoreAndRequestDrawingArea)
            {
                remainingArea = remainingArea.DecreaseHeight(result.layout.BoundingBox.Height);
                layouts = [.. layouts, result.layout.SetOffset(new Position(0, yOffset))];
                yOffset += result.layout.BoundingBox.Height;
            }

            if (sectionProcessingInfo is ProcessingInfo.NewPageRequired
                or ProcessingInfo.RequestDrawingArea
                or ProcessingInfo.IgnoreAndRequestDrawingArea)
            {
                break;
            }
        }

        Rectangle boudingBox = layouts
           .Select(l => l.BoundingBox)
           .DefaultIfEmpty(Rectangle.Empty)
           .CalculateBoundingBox();

        LayoutPartition layoutPartition = sectionProcessingInfo.CalculateLayoutPartition(previousSectionLayout.Partition);

        SectionLayout sectionLayout = new(
            section.Id,
            layouts,
            boudingBox,
            Borders.None,
            layoutPartition
        );

        return (sectionLayout, sectionProcessingInfo);
    }

    public static (SectionLayout, ProcessingInfo) Update(
        this SectionLayout sectionLayout,
        Section section,
        SectionLayout previousSectionLayout,
        Size availableArea,
        FieldVariables fieldVariables,
        LayoutServices services)
    {
        Size remainingArea = availableArea;
        Layout[] updatedLayouts = [];
        ProcessingInfo sectionProcessingInfo = ProcessingInfo.Done;
        float yOffset = 0;

        foreach (Layout layout in sectionLayout.Layouts)
        {
            (Layout layout, ProcessingInfo processingInfo) result = layout switch
            {
                ParagraphLayout pl => pl.Update(
                    section.Find<Paragraph>(pl.ModelId),
                    previousSectionLayout.Layouts.LastOfTypeOr(ParagraphLayout.Empty), // try find in previous section
                    remainingArea,
                    fieldVariables,
                    services
                ),
                _ => (NoLayout.Instance, ProcessingInfo.Ignore)
            };

            updatedLayouts = [.. updatedLayouts, result.layout.SetOffset(new Position(0, yOffset))];
            yOffset += result.layout.BoundingBox.Height;
        }

        Rectangle boudingBox = updatedLayouts
            .CalculateBoundingBox(Rectangle.Empty);

        bool isSectionFinished = updatedLayouts.Last().ModelId == section.Elements.Last().Id
            && updatedLayouts.Last().Partition.IsFinished();

        LayoutPartition lp = sectionProcessingInfo.CalculateLayoutPartitionAfterUpdate(previousSectionLayout.Partition, isSectionFinished);

        SectionLayout updatedSection = new(
            section.Id,
            updatedLayouts,
            boudingBox,
            Borders.None,
            lp
        );

        return (updatedSection, sectionProcessingInfo);
    }

    //public static SectionLayoutingResult Process(
    //        this Section section,
    //        SectionLayoutingResult previousLayoutingResult,
    //        Rectangle drawingPageArea,
    //        FieldVariables fieldVariables,
    //        LayoutServices services)
    //{
    //    Model[] unprocessed = section
    //            .Elements
    //            .SkipProcessed(previousLayoutingResult.LastModelLayoutingResult);

    //    Layout[] layouts = [];
    //    Rectangle remainingArea = drawingPageArea.MoveTo(Position.Zero);
    //    LayoutingResult lastModelResult = previousLayoutingResult.LastModelLayoutingResult;
    //    ResultStatus resultStatus = ResultStatus.Finished;

    //    foreach (Model model in unprocessed)
    //    {
    //        LayoutingResult lr = model switch
    //        {
    //            Paragraph paragraph => paragraph.Process(lastModelResult, remainingArea, fieldVariables, services),
    //            Table table => table.Process(lastModelResult, remainingArea, fieldVariables, services),
    //            _ => NoLayoutingResult.Create(remainingArea)
    //        };

    //        if(lr.Status is ResultStatus.Finished or ResultStatus.RequestDrawingArea or ResultStatus.NewPageRequired)
    //        {
    //            remainingArea = lr.RemainingDrawingArea;
    //            layouts = [.. layouts, .. lr.Layouts.OfType<IIdLayout>().Where(l => l.IsNotEmpty).Cast<Layout>()];
    //            lastModelResult = lr;
    //        }

    //        if(lr.Status != ResultStatus.Finished)
    //        {
    //            resultStatus = ResultStatus.RequestDrawingArea;
    //            break;
    //        }
    //    }

    //    Rectangle boudingBox = layouts
    //        .Select(l => l.BoundingBox)
    //        .DefaultIfEmpty(new Rectangle(drawingPageArea.TopLeft, Size.Zero))
    //        .CalculateBoundingBox();

    //    LayoutPartition partition = resultStatus.CalculateLayoutPartition(previousLayoutingResult);

    //    Rectangle remArea = drawingPageArea
    //        .CropFromTop(boudingBox.Height);

    //    return new SectionLayoutingResult(
    //        section.Id,
    //        new SectionLayout(section.Id, layouts, boudingBox, Borders.None, partition),
    //        lastModelResult,
    //        remArea,
    //        resultStatus
    //    );
    //}

    //public static SectionLayoutingResult UpdateLayout(
    //    this SectionLayout sectionLayout,
    //    Section section,
    //    Rectangle drawingPageArea,
    //    FieldVariables fieldVariables,
    //    LayoutServices services)
    //{
    //    Rectangle remainingArea = drawingPageArea.MoveTo(Position.Zero);
    //    Layout[] updatedLayouts = [];
    //    LayoutingResult layoutingResult = NoLayoutingResult.Create(remainingArea);

    //    foreach (Layout layout in sectionLayout.Layouts)
    //    {
    //        switch(layout)
    //        {
    //            case ParagraphLayout paragraphLayout:
    //                Paragraph paragraph = section.Elements
    //                    .OfType<Paragraph>()
    //                    .Single(e => e.Id == paragraphLayout.ModelId);
    //                ParagraphLayoutingResult plr = paragraphLayout.Update(paragraph, remainingArea, fieldVariables, services);
    //                updatedLayouts = [.. updatedLayouts, plr.ParagraphLayout];
    //                remainingArea = remainingArea.CropFromTop(plr.ParagraphLayout.BoundingBox.Height);
    //                break;
    //            // case TableLayout tableLayout:
    //                // tableLayout.UpdateLayout(section, drawingPageArea, fieldVariables, services);
    //                // break;
    //        }
    //    }

    //    Rectangle bb = updatedLayouts
    //        .Select(l => l.BoundingBox)
    //        .DefaultIfEmpty(Rectangle.Empty)
    //        .CalculateBoundingBox()
    //        .MoveTo(drawingPageArea.TopLeft);

    //    SectionLayout sl = new(
    //        section.Id,
    //        updatedLayouts,
    //        bb,
    //        Borders.None,
    //        LayoutPartition.StartEnd
    //    );

    //    return new SectionLayoutingResult(section.Id, sl, layoutingResult, remainingArea, ResultStatus.Finished);
    //}
}

file static class SectionOperators
{
    public static T Find<T>(this Section section, ModelId id) where T : Model =>
        section.Elements.OfType<T>().Single(e => e.Id == id);

    public static T LastOfTypeOr<T>(this Layout[] layouts, T ifNone) where T : Layout =>
        layouts switch
        {
            [.., T last] => last,
            _ => ifNone,
        };

    public static ParagraphLayout TryFindParagraphLayout(this SectionLayout sectionLayout, ModelId modelId) =>
        sectionLayout.Layouts.OfType<ParagraphLayout>().SingleOrDefault(p => p.ModelId == modelId, ParagraphLayout.Empty);

    public static Model[] Unprocessed(this Section section, Layout[] previousLayouts) =>
        previousLayouts.Length == 0
            ? section.Elements
            : [..section.Elements.SkipFinished(previousLayouts.Last())];

    private static IEnumerable<Model> SkipFinished(this Model[] models, Layout lastLayout) =>
        models.SkipProcessed(lastLayout.ModelId, lastLayout.Partition.IsFinished());
}
