using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Paragraphs.Builders;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements;
using Proxoft.DocxToPdf.Models.Styles.Paragraphs;
using Proxoft.DocxToPdf.Models.Styles.Services;

using C = Proxoft.DocxToPdf.Core.Structs;

using static Proxoft.DocxToPdf.Models.FieldUpdateResult;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements.Drawings;
using Proxoft.DocxToPdf.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs;

internal class Paragraph(
    IEnumerable<LineElement> elements,
    IEnumerable<FixedDrawing> fixedDrawings,
    IStyleFactory styleFactory) : PageContextElement
{
    private readonly IStyleFactory _styleFactory = styleFactory;

    private Line[] _lines = [];
    private readonly FixedDrawing[] _fixedDrawings = [.. fixedDrawings];
    private readonly Stack<LineElement> _unprocessedElements = elements.ToStack();
    private C.Point _pageOffset = C.Point.Zero;

    private ParagraphStyle ParagraphStyle => _styleFactory.ParagraphStyle;

    public double SpaceBefore => this.ParagraphStyle.Spacing.Before;

    public double SpaceAfter => this.ParagraphStyle.Spacing.After;

    public override void Prepare(PageContext pageContext, Func<PagePosition, PageContextElement, PageContext> nextPageContextFactory)
    {
        ExecuteResult execResult;
        int continueOnLineIndex = 0;

        this.PrepareFixedDrawings(pageContext, nextPageContextFactory);

        var context = pageContext;
        do
        {
            (execResult, continueOnLineIndex) = this.ExecutePrepare(context, continueOnLineIndex);
            if (execResult == ExecuteResult.RequestNextPage)
            {
                this.SetPageRegion(new PageRegion(context.PagePosition, context.Region));
                context = nextPageContextFactory(context.PagePosition, this);
            }
        } while (execResult != ExecuteResult.Done);

        var heightInLastRegion = _lines
            .Where(l => l.Position.Page == context.PagePosition)
            .Sum(l => l.HeightWithSpacing);

        this.SetPageRegion(new PageRegion(context.PagePosition, new C.Rectangle(context.Region.TopLeft, context.Region.Width, heightInLastRegion)));
    }

    private void PrepareFixedDrawings(PageContext context, Func<PagePosition, PageContextElement, PageContext> nextPageContextFactory)
    {
        var currentContext = context;
        var unprocessed = _fixedDrawings.ToList();

        var paragraphYOffset = 0.0;
        while(unprocessed.Count > 0)
        {
            var fitsInContext = unprocessed
                .Where(fd => fd.OffsetFromParent.Y < currentContext.Region.BottomY
                          && fd.OffsetFromParent.Y + fd.Size.Height < currentContext.Region.BottomY)
                .ToArray();

            foreach(var fd in fitsInContext)
            {
                var y = Math.Max(0, fd.OffsetFromParent.Y - paragraphYOffset);
                var position = currentContext
                    .TopLeft
                    .MoveX(fd.OffsetFromParent.X)
                    .MoveY(y);

                fd.SetPosition(position);
            }

            unprocessed.RemoveAll(fd => fitsInContext.Any(f => f == fd));
            if(unprocessed.Count > 0)
            {
                paragraphYOffset += currentContext.Region.Height;
                currentContext = nextPageContextFactory(context.PagePosition.Next(), this);
            }
        }
    }

    private (ExecuteResult, int) ExecutePrepare(PageContext context, int fromLineIndex)
    {
        var yOffset = 0.0;

        // update lines
        var i = fromLineIndex;
        while (i < _lines.Length)
        {
            var line = _lines[i];

            var updateResult = line.Update(context.PageVariables);
            if(updateResult == ReconstructionNecessary)
            {
                this.ClearLinesFromIndex(i);
                break;
            }

            if(context.Region.Y + yOffset + line.HeightWithSpacing > context.Region.BottomY)
            {
                return (ExecuteResult.RequestNextPage, i);
            }

            line.SetPosition(context.TopLeft.MoveY(yOffset));
            yOffset += line.HeightWithSpacing;
            i++;
        }

        var defaultLineHeight = _styleFactory.TextStyle.Font.Height;
        var relativeYOffset = _lines.Sum(l => l.HeightWithSpacing);

        while (_unprocessedElements.Count > 0)
        {
            var line = _unprocessedElements.CreateLine(
                this.ParagraphStyle.LineAlignment,
                relativeYOffset,
                _fixedDrawings,
                context.Region.Width,
                defaultLineHeight,
                context.PageVariables,
                this.ParagraphStyle.Spacing.Line);

            _lines = [
                .._lines,
                line
            ];

            if (context.Region.Y + yOffset + line.HeightWithSpacing > context.Region.BottomY)
            {
                return (ExecuteResult.RequestNextPage, _lines.Length - 1);
            }

            line.SetPosition(context.TopLeft.MoveY(yOffset));
            yOffset += line.HeightWithSpacing;
            relativeYOffset += line.HeightWithSpacing;
        }

        return (ExecuteResult.Done, 0);
    }

    public override void Render(IRenderer renderer)
    {
        foreach(var fixedDrawing in _fixedDrawings)
        {
            var page = renderer.GetPage(fixedDrawing.Position.Page.PageNumber).Offset(_pageOffset);
            fixedDrawing.Render(page);
        }

        foreach(var line in _lines)
        {
            var page = renderer.GetPage(line.Position.Page.PageNumber).Offset(_pageOffset);
            line.Render(page);
        }

        this.RenderBorders(renderer, renderer.Options.ParagraphBorders);
    }

    private void ClearLinesFromIndex(int fromIndex)
    {
        LineElement[] elements = [
            .._lines
                .Skip(fromIndex)
                .Reverse()
                .SelectMany(l => l.GetAllElements())
        ];

        _lines =[.._lines.Take(fromIndex)];

        _unprocessedElements.Push(elements);
    }

    public override void SetPageOffset(C.Point pageOffset)
    {
        _pageOffset = pageOffset;
    }

    private enum ExecuteResult
    {
        RequestNextPage,
        Done
    }
}
