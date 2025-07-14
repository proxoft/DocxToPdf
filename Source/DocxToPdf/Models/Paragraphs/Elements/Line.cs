using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;
using Proxoft.DocxToPdf.Models.Styles;
using static Proxoft.DocxToPdf.Models.Core.FieldUpdateResult;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements;

internal class Line: ParagraphElementBase
{
    private readonly LineSegment[] _segments;
    private readonly LineSpacing _lineSpacing;

    public Line(IEnumerable<LineSegment> segments, LineSpacing lineSpacing)
    {
        _segments = [.. segments];

        Rectangle boundingRectangle = Rectangle.Union(segments.Select(s => s.PageRegion));
        this.Size = boundingRectangle.Size;
        _lineSpacing = lineSpacing;
    }

    public double HeightWithSpacing
        => this.Size.Height + _lineSpacing.CalculateSpaceAfterLine(this.Size.Height);

    public override void SetPosition(DocumentPosition position)
    {
        base.SetPosition(position);
        foreach (LineSegment segment in _segments)
        {
            segment.SetPosition(position);
        }
    }

    public FieldUpdateResult Update(PageVariables variables)
    {
        foreach (LineSegment segment in _segments)
        {
            FieldUpdateResult result = segment.Update(variables);
            if(result == ReconstructionNecessary)
            {
                return result;
            }
        }

        return NoChange;
    }

    public override void Render(IRendererPage page)
    {
        _segments.Render(page);
    }

    public IEnumerable<LineElement> GetAllElements() =>
        _segments.SelectMany(s => s.GetAllElements());

    //public FieldUpdateResult UpdateFields()
    //{
    //    // var updateResult = _segments.UpdateFields();
    //    return NoChange;
    //}
}
