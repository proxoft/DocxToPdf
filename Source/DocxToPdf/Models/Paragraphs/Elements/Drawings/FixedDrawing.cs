using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements.Drawings;

internal class FixedDrawing : ParagraphElementBase
{
    private readonly string _imageId;
    private readonly Margin _margin;
    private readonly IImageAccessor _imageAccessor;

    public FixedDrawing(
        string imageId,
        Point offsetFromParent,
        Size size,
        Margin margin,
        IImageAccessor imageAccessor)
    {
        _imageId = imageId;
        this.OffsetFromParent = offsetFromParent;
        _margin = margin;
        _imageAccessor = imageAccessor;

        this.Size = size;
    }

    public Rectangle BoundingBox => new(this.OffsetFromParent + _margin.TopLeftReverseOffset(), this.Size.Expand(_margin.HorizontalMargins, _margin.VerticalMargins));

    public Point OffsetFromParent { get; }

    public override void Render(IRendererPage page)
    {
        var stream = _imageAccessor.GetImageStream(_imageId);
        page.RenderImage(stream, this.Position.Offset, this.Size);
    }
}
