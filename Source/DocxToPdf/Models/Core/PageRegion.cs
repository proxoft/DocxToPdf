using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;

namespace Proxoft.DocxToPdf.Models;

internal class PageRegion(PagePosition pagePosition, Rectangle region)
{
    public static readonly PageRegion None = new(PagePosition.None, Rectangle.Empty);

    public PagePosition PagePosition { get; } = pagePosition;

    public Rectangle Region { get; } = region;
}
