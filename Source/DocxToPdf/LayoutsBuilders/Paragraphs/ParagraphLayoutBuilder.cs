using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts.Paragraphs;
using Proxoft.DocxToPdf.LayoutsBuilders.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class ParagraphLayoutBuilder
{
    private static readonly float _defaultLineHeight = 12f; // Default line height, can be adjusted based on font size or other factors

    public static ParagraphLayoutingResult Process(
        this Paragraph paragraph,
        ParagraphLayoutingResult previousLayoutingResult,
        Rectangle availableArea,
        LayoutServices services)
    {
        ModelId continueFrom = previousLayoutingResult.Status == ResultStatus.Finished
            ? ModelId.None
            : previousLayoutingResult.StartFromElementId;

        IEnumerable<Element> unprocessed = paragraph.Elements
            .SkipProcessed(continueFrom);

        (LineLayout[] lines, ModelId lastProcessedElementId, ResultStatus status) = unprocessed.CreateLines(availableArea, services);

        Rectangle paragraphBb = lines
            .Select(l => l.BoundingBox)
            .DefaultIfEmpty(new Rectangle(availableArea.X, availableArea.Y, 0, 0))
            .CalculateBoundingBox();

        Rectangle remainingArea = Rectangle.FromCorners(paragraphBb.BottomLeft, availableArea.BottomRight);
        ParagraphLayout pl = new([.. lines], paragraphBb, Borders.None);
        return new ParagraphLayoutingResult(
            paragraph.Id,
            pl,
            lastProcessedElementId,
            remainingArea,
            status
        );
    }

    private static (LineLayout[] lines, ModelId lastProcessedElementId, ResultStatus status) CreateLines(
        this IEnumerable<Element> elements,
        Rectangle availableArea,
        LayoutServices services)
    {
        Element[] unprocessed = [..elements];
        bool isEmpty = unprocessed.Length == 0;

        List<LineLayout> lines = [];
        ModelId lastProcessed = ModelId.None;

        bool keepProcessing = true;
        float remainingHeight = availableArea.Height;

        Position currentPosition = availableArea.TopLeft;
        do
        {
            (LineLayout line, ModelId lastElementId) = unprocessed.CreateLine(currentPosition, availableArea.Width, services);
            remainingHeight -= line.BoundingBox.Height;

            if (remainingHeight >= 0)
            {
                lines.Add(line);
                lastProcessed = lastElementId;
                unprocessed = [..unprocessed.SkipWhile(e => e.Id != lastElementId).Skip(1)];
                currentPosition = currentPosition.ShiftY(line.BoundingBox.Height);
            }
            else
            {
                keepProcessing = false;
            }

            keepProcessing &= unprocessed.Length > 0;
        } while (keepProcessing);


        ResultStatus status = 
            lines.Count == 0 ? ResultStatus.Ignore
            : unprocessed.Length > 0 ? ResultStatus.RequestDrawingArea
            : ResultStatus.Finished;

        return ([..lines], lastProcessed, status);
    }

    private static (LineLayout line, ModelId lastElementId) CreateLine(
        this Element[] elements,
        Position startPosition,
        float availableWidth,
        LayoutServices services)
    {
        float lineWidth = 0;
        ElementLayout[] elementLayouts = [];
        ModelId lastElementId = ModelId.None;
        bool interrupted = false;
        Position currentPosition = startPosition;

        foreach (Element element in elements)
        {
            ElementLayout elementLayout = element.CreateLayout(currentPosition, services);
            if (lineWidth + elementLayout.BoundingBox.Width > availableWidth)
            {
                interrupted = true;
                break;
            }

            lastElementId = element.Id;
            elementLayouts = [..elementLayouts, elementLayout];
            currentPosition = new Position(elementLayout.BoundingBox.Right, currentPosition.Y);
            lineWidth += elementLayout.BoundingBox.Width;
        }

        // TODO: known issue: if element does not fit in line, word wrap must be implemented
        return elementLayouts.Length == 0
            ? (CreateEmptyLine(startPosition), lastElementId)
            : (elementLayouts.CreateLine(!interrupted), lastElementId);
    }

    private static LineLayout CreateEmptyLine(Position position)
    {
        Rectangle bb = new(position, new Size(0, _defaultLineHeight));
        return new LineLayout([], true, bb, Borders.None);
    }

    private static LineLayout CreateLine(this ElementLayout[] elements, bool isLast)
    {
        float height = elements
            .Select(e => e.BoundingBox.Height)
            .DefaultIfEmpty(_defaultLineHeight)
            .Max();

        Rectangle bb = elements.Select(e => e.BoundingBox).CalculateBoundingBox();
        return new LineLayout(elements, isLast, bb, Borders.None);
    }
}
