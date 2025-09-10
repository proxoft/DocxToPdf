using System;
using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using WDrawing = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;

internal static class DrawingExtensions
{
    public static bool IsFixedDrawing(this Word.Drawing drawing) =>
        drawing.Anchor is not null;

    public static bool IsInlineDrawing(this Word.Drawing drawing) =>
        drawing.Inline is not null;

    public static InlineDrawing[] CreateInlineDrawing(
        this Word.Drawing drawing,
        TextStyle textStyle,
        BuilderServices builderServices)
    {
        if (drawing.Inline == null)
        {
            return [];
        }

        Size size = drawing.Inline.Extent.ToSize();
        OpenXml.Drawing.Blip blipElement = drawing.Inline.Descendants<OpenXml.Drawing.Blip>().First();
        byte[] image = builderServices.ImageAccessor.GetImageBytes(blipElement.Embed?.Value ?? "");

        InlineDrawing inlineDrawing = new(
            builderServices.IdFactory.NextDrawingId(),
            size,
            textStyle,
            image
        );

        return [inlineDrawing];
    }

    public static FixedDrawing[] CreateFixedDrawing(this Word.Drawing drawing, BuilderServices services)
    {
        if(drawing.Anchor is null)
        {
            return [];
        }

        Padding margin = drawing.Anchor.ToPadding();
        Size size = drawing.Anchor.Extent.ToSize();
        OpenXml.Drawing.Blip blipElement = drawing.Anchor.Descendants<OpenXml.Drawing.Blip>().First();
        byte[] image = services.ImageAccessor.GetImageBytes(blipElement.Embed?.Value ?? "");

        Position position = drawing.Anchor.ToPosition();
        FixedDrawing fixedDrawing = new(
            services.IdFactory.NextDrawingId(),
            image,
            position,
            size,
            margin
        );

        return [fixedDrawing];
    }

    private static Size ToSize(this WDrawing.Extent? extent)
    {
        float width = extent?.Cx.EmuToPoint() ?? 0;
        float height = extent?.Cy.EmuToPoint() ?? 0;
        return new Size(width, height);
    }

    private static Padding ToPadding(this WDrawing.Anchor anchor)
    {
        float top = anchor.DistanceFromTop.EmuToPoint();
        float right = anchor.DistanceFromRight.EmuToPoint();
        float bottom = anchor.DistanceFromBottom.EmuToPoint();
        float left = anchor.DistanceFromLeft.EmuToPoint();

        return new Padding(left, top, right, bottom);
    }

    private static Position ToPosition(this WDrawing.Anchor anchor) =>
       (anchor.SimplePos?.Value ?? false)
            ? new Position(anchor.SimplePosition?.X?.Value ?? 0, anchor.SimplePosition?.Y?.Value ?? 0)
            : new Position(anchor.HorizontalPosition?.PositionOffset.ToFloat() ?? 0, anchor.VerticalPosition?.PositionOffset.ToFloat() ?? 0);

    private static float ToFloat(this WDrawing.PositionOffset? positionOffset)
    {
        if (positionOffset is null)
        {
            return 0;
        }

        long offset = Convert.ToInt64(positionOffset.Text);
        return offset.EmuToPoint();
    }
}
