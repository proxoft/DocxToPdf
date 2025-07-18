using System;
using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Sections;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Layouts;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class SectionLayoutBuilder
{
    public static LayoutingResult Process(
        this Section section,
        LastProcessed alreadyProcessed,
        Rectangle drawingPageArea,
        LayoutServices services)
    {
        // split drawing page Area to columns
        // skip elements using startFrom
        Stack<Model> toProcess = section.Elements
            .SkipProcessed(alreadyProcessed.Current)
            .ToStackReversed();

        Layout[] layouts = [];

        Rectangle remainingArea = drawingPageArea;
        LastProcessed lastProcessed = LastProcessed.None;
        ResultStatus status = ResultStatus.Finished;

        while (toProcess.Count > 0 && status == ResultStatus.Finished)
        {
            Model model = toProcess.Pop();
            switch (model)
            {
                case Paragraph p:
                    LayoutingResult result = p.Process(alreadyProcessed.Childs(), remainingArea, services);
                    layouts = [..layouts, ..result.Layouts];
                    remainingArea = result.RemainingDrawingArea;
                    status = result.Status;
                    lastProcessed = result.LastProcessed;
                    break;
            };
        }

        return new LayoutingResult(
            layouts,
            lastProcessed,
            ModelReference.None,
            remainingArea,
            status
        );
    }
}
