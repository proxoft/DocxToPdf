using System.Collections.Generic;
using System.Linq;
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
        ModelReference startFrom,
        Rectangle drawingPageArea,
        LayoutServices services)
    {
        // split drawing page Area to columns
        // skip elements using startFrom
        Stack<Model> toProcess = section.Elements.ToStackReversed();

        Layout[] layouts = [];

        Rectangle remainingArea = drawingPageArea;

        while (toProcess.Count > 0)
        {
            Model model = toProcess.Pop();
            switch (model)
            {
                case Paragraph p:
                    LayoutingResult result = p.Process(ModelReference.None, remainingArea, services);
                    layouts = [..layouts, ..result.Layouts];
                    remainingArea = result.RemainingDrawingArea;
                    break;
            };
        }

        return new LayoutingResult(layouts, ModelReference.None, remainingArea);
    }
}
