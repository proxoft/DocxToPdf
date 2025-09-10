using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Layouts.Paragraphs;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Paragraphs;

internal static class FixedImageLayoutBuilder
{
    public static FixedImageLayout[] CreateFixedImageLayouts(
        this FixedDrawing[] fixedDrawings,
        Size availableSize,
        FixedImageLayout[] processed) =>
        [
            ..fixedDrawings
                .Where(f => processed.All(p => p.ModelId != f.Id))
                .Where(f => f.FitsInArea(availableSize))
                .Select(f => f.CreateFixedImageLayout())
        ];

    public static FixedImageLayout[] Update(
        this FixedImageLayout[] existingLayouts,
        FixedDrawing[] fixedDrawings,
        Size availableArea) =>
    [
        ..fixedDrawings
            .Where(f => existingLayouts.Any(el => el.ModelId == f.Id))
            .Where(f => f.FitsInArea(availableArea))
            .Select(f => f.CreateFixedImageLayout())
    ];

    public static Rectangle[] CreateReservedSpaces(this FixedDrawing[] fixedDrawings) =>
        [..fixedDrawings.Select(f => new Rectangle(f.Offset, f.Size).Expand(f.TextDistance))];

    private static FixedImageLayout CreateFixedImageLayout(this FixedDrawing fixedDrawing) =>
        new(
            fixedDrawing.Id,
            fixedDrawing.Image,
            new Rectangle(fixedDrawing.Offset, fixedDrawing.Size),
            Borders.None
        );

    private static bool FitsInArea(this FixedDrawing fixedDrawing, Size availableArea) =>
        fixedDrawing.Size.Width <= availableArea.Width && fixedDrawing.Size.Height <= availableArea.Height;
}
