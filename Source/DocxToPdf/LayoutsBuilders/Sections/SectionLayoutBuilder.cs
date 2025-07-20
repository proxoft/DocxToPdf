using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Extensions;
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
        // split drawing page Area to columns
        // skip elements using startFrom

        Stack<Model> toProcess = section.Elements
            .SkipProcessed(previousLayoutingResult.LastProcessedModel, finished: previousLayoutingResult.LastModelLayoutingResult.Status == ResultStatus.Finished)
            .ToStackReversed();

        Layout[] layouts = [];

        Rectangle remainingArea = drawingPageArea;
        ResultStatus status = ResultStatus.Finished;

        ModelId lastModelId = ModelId.None;
        LayoutingResult modelLayoutingResult = NoLayoutingResult.Instance;

        while (toProcess.Count > 0 && status == ResultStatus.Finished)
        {
            Model model = toProcess.Pop();
            switch (model)
            {
                case Paragraph paragraph:
                    {
                        ParagraphLayoutingResult paragraphResult = previousLayoutingResult.LastProcessedModel == paragraph.Id
                            ? (ParagraphLayoutingResult)previousLayoutingResult.LastModelLayoutingResult
                            : ParagraphLayoutingResult.New(remainingArea);

                        modelLayoutingResult = paragraph.Process(paragraphResult, remainingArea, services);
                    }
                    break;
                case Table table:
                    {
                        TableLayoutingResult tableResult = previousLayoutingResult.LastProcessedModel == table.Id
                            ? (TableLayoutingResult)previousLayoutingResult.LastModelLayoutingResult
                            : TableLayoutingResult.New(remainingArea);

                        modelLayoutingResult = table.Process(tableResult, remainingArea, services);
                    }
                    break;
            };

            layouts = [.. layouts, .. modelLayoutingResult.Layouts];
            remainingArea = modelLayoutingResult.RemainingDrawingArea;
            status = modelLayoutingResult.Status;

            lastModelId = model.Id;
        }

        return new SectionLayoutingResult(
            layouts,
            lastModelId,
            modelLayoutingResult,
            remainingArea,
            status
        );
    }
}
