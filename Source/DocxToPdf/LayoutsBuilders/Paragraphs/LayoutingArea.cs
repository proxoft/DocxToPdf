using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal record LayoutingArea(
    Size AvailableSize,
    Rectangle[] Reserved
);

internal record ParagraphLayoutingArea(
    Size AvailableSize,
    float YOffset,
    float LineParagraphYOffset,
    Rectangle[] Reserved)
: LayoutingArea(AvailableSize, Reserved);

internal static class ParagraphLayoutingAreaOperators
{
    public static ParagraphLayoutingArea ProceedBy(this ParagraphLayoutingArea area, float height) =>
        area with
        {
            YOffset = area.YOffset + height,
            LineParagraphYOffset = area.LineParagraphYOffset + height,
            AvailableSize = area.AvailableSize.DecreaseHeight(height)
        };
}
