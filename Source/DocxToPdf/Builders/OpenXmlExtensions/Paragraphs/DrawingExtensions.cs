using System;
using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Paragraphs.Drawings;
using Proxoft.DocxToPdf.Documents.Styles.Texts;
using Proxoft.DocxToPdf.Models.Common;
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

        Margin margin = drawing.Anchor.ToMargin();
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

    private static Margin ToMargin(this WDrawing.Anchor anchor)
    {
        double top = anchor.DistanceFromTop.EmuToPoint();
        double right = anchor.DistanceFromRight.EmuToPoint();
        double bottom = anchor.DistanceFromBottom.EmuToPoint();
        double left = anchor.DistanceFromLeft.EmuToPoint();

        return new Margin(top, right, bottom, left);
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
