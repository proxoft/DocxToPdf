using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class GridOperators
{
    public static Rectangle[] GridAvailableAreas(this Grid grid, Rectangle totalAvailableArea)
    {
        float[] columnWidths = grid.TotalGridWidth() <= totalAvailableArea.Width
            ? grid.ColumnWidths
            : grid.RecalculateWidthsToFitIn(totalAvailableArea.Width);

        float x = totalAvailableArea.X;
        float y = totalAvailableArea.Y;
        float height = totalAvailableArea.Height;

        float offset = 0;
        Rectangle[] areas = [
            ..columnWidths
                .Select(width => {
                    Rectangle r = new(x + offset, y, width, height);
                    offset += width;
                    return r;
                })
        ];

        return areas;
    }

    public static (float offset, float width) CalculateCellRegion(this Grid grid, GridPosition gridPosition)
    {
        float offset = grid.CalculateCellXOffset(gridPosition);
        float width = grid.CalculateCellWidth(gridPosition);
        return (offset, width);
    }

    private static float CalculateCellXOffset(this Grid grid, GridPosition gridPosition) =>
        grid.ColumnWidths
            .Take(gridPosition.Column - 1)
            .Sum();

    private static float CalculateCellWidth(this Grid grid, GridPosition gridPosition) =>
        grid.ColumnWidths
            .Skip(gridPosition.Column)
            .Take(gridPosition.ColumnSpan)
            .Sum();

    private static float TotalGridWidth(this Grid grid) =>
        grid.ColumnWidths.Sum();

    private static float[] RecalculateWidthsToFitIn(this Grid grid, float maxWidth)
    {
        float totalWidth = grid.TotalGridWidth();
        float aggregatedWidth = 0;
        float[] widths = [
            ..grid.ColumnWidths
            .Select((width, index) =>
            {
                float ratio = width / totalWidth;
                float calculatedWidth = index == grid.ColumnWidths.Length - 1
                    ? maxWidth - aggregatedWidth
                    : maxWidth * ratio;

                aggregatedWidth += calculatedWidth;
                return calculatedWidth;
            })
        ];

        return widths;
    }
}
