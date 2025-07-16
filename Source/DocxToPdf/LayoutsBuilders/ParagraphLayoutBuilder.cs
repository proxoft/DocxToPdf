using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Layouts;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsBuilders;

internal static class ParagraphLayoutBuilder
{
    public static LayoutingResult Process(
        this Paragraph paragraph,
        ModelReference startFrom,
        Rectangle availableArea,
        LayoutServices services)
    {
        Stack<Element> unprocessed = paragraph.Elements.ToStackReversed();

        Rectangle area = availableArea;
        Position position = availableArea.Position;

        ModelReference paragraphReference = ModelReference.New(paragraph.Id);

        List<LineLayout> lines = [];

        float remainingWidth = availableArea.Width;
        float remainingHeight = availableArea.Height;
        Position currentPosition = availableArea.Position;

        ElementLayout[] lineElements = [];
        while (unprocessed.Count > 0)
        {
            Element element = unprocessed.Pop();
            Size size = services.CalculateBoundingBox(element);

            if(size.Width > remainingWidth || size.Height > remainingHeight)
            {
                LineLayout lineLayout = lineElements.CreateLine(paragraphReference);
                lines.Add(lineLayout);
                lineElements = [];

                bool interrupt = size.Height > remainingHeight;

                remainingHeight -= lineLayout.BoundingBox.Height;
                remainingWidth = availableArea.Width;
                currentPosition = new Position(availableArea.Position.X, availableArea.Height)
                    .ShiftY(-remainingHeight);

                unprocessed.Push(element);
                if (interrupt)
                {
                    break;
                }
            }
            else
            {
                Rectangle bb = new(currentPosition, size);
                ElementLayout el = element switch
                {
                    Text t => new TextLayout(ModelReference.New(paragraph.Id, element.Id), bb, t),
                    _ => new EmptyLayout(ModelReference.New(paragraph.Id, element.Id), bb)
                };

                lineElements = [.. lineElements, el];
                currentPosition = currentPosition.ShiftX(size.Width);
                remainingWidth -= size.Width;
            }
        }

        if (lineElements.Length > 0)
        {
            LineLayout lineLayout = lineElements.CreateLine(paragraphReference);
            lines.Add(lineLayout);
        }

        Rectangle paragraphBb = lines
            .Select(l => l.BoundingBox)
            .CalculateBoundingBox();

        return new LayoutingResult([..lines], ModelReference.None, availableArea.CropFromTop(paragraphBb.Height));
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
        return new LineLayout(paragraphReference, elements, bb);
    }
}
