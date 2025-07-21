using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Borders;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class ParagraphLayoutBuilder
{
    public static ParagraphLayoutingResult Process(
        this Paragraph paragraph,
        ParagraphLayoutingResult previousLayoutingResult,
        Rectangle availableArea,
        LayoutServices services)
    {
        ModelId continueFrom = previousLayoutingResult.Status == ResultStatus.Finished
            ? ModelId.None
            : previousLayoutingResult.StartFromElementId;

        Stack<Element> unprocessed = paragraph.Elements
            .SkipProcessed(continueFrom)
            .ToStackReversed();

        Rectangle area = availableArea;
        Position position = availableArea.TopLeft;

        ModelReference paragraphReference = ModelReference.New(paragraph.Id);

        List<LineLayout> lines = [];

        float remainingWidth = availableArea.Width;
        float remainingHeight = availableArea.Height;
        Position currentPosition = availableArea.TopLeft;

        ElementLayout[] lineElements = [];
        ModelId lastProcessedElementId = ModelId.None;
        ResultStatus status = ResultStatus.Finished;

        while (unprocessed.Count > 0)
        {
            Element element = unprocessed.Pop();
            (Size boundingBox, float baseLineOffset) = services.CalculateBoundingSizeAndBaseline(element);

            if(boundingBox.Width > remainingWidth || boundingBox.Height > remainingHeight)
            {
                bool interrupt = boundingBox.Height > remainingHeight;

                if (lineElements.Length > 0)
                {
                    LineLayout lineLayout = lineElements.CreateLine(paragraphReference);
                    lines.Add(lineLayout);
                    remainingHeight -= lineLayout.BoundingBox.Height;
                    currentPosition = new Position(availableArea.TopLeft.X, currentPosition.Y)
                        .ShiftY(lineLayout.BoundingBox.Height);
                }

                lineElements = [];
                remainingWidth = availableArea.Width;

                unprocessed.Push(element);
                if (interrupt)
                {
                    status = ResultStatus.RequestDrawingArea;
                    break;
                }
            }
            else
            {
                Rectangle bb = new(currentPosition, boundingBox);
                ElementLayout el = element switch
                {
                    Text t => new TextLayout(ModelReference.New(paragraph.Id, element.Id), bb, baseLineOffset, t, Borders.None),
                    _ => new EmptyLayout(ModelReference.New(paragraph.Id, element.Id), bb, Borders.None)
                };

                lineElements = [.. lineElements, el];
                currentPosition = currentPosition.ShiftX(boundingBox.Width);
                remainingWidth -= boundingBox.Width;
                lastProcessedElementId = element.Id;
            }
        }

        if (lineElements.Length > 0)
        {
            LineLayout lineLayout = lineElements.CreateLine(paragraphReference);
            lines.Add(lineLayout);
        }

        Rectangle paragraphBb = lines
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(Rectangle.Empty) // create empty line
            .CalculateBoundingBox();

        ParagraphLayout pl = new(paragraphReference, [.. lines], paragraphBb, Borders.None);
        return new ParagraphLayoutingResult(
            [pl],
            lastProcessedElementId,
            availableArea.CropFromTop(paragraphBb.Height),
            status
        );
    }

    private static LineLayout CreateLine(
        this ElementLayout[] elements,
        ModelReference paragraphReference)
    {
        float height = elements
            .Select(e => e.BoundingBox.Height)
            .DefaultIfEmpty(0)
            .Max();

        Rectangle bb = elements.Select(e => e.BoundingBox).CalculateBoundingBox();
        return new LineLayout(paragraphReference, elements, bb, Borders.None);
    }
}
