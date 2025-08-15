using System.Linq;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Tables;
using Proxoft.DocxToPdf.Layouts.Tables;

namespace Proxoft.DocxToPdf.LayoutsBuilders.Tables;

internal static class GridLayoutOperators
{
    public static Rectangle[] GridAvailableAreas(this GridLayout grid, Size totalAvailableArea)
    {
        float[] columnWidths = grid.TotalGridWidth() <= totalAvailableArea.Width
            ? grid.Columns
            : grid.RecalculateWidthsToFitIn(totalAvailableArea.Width);

        float x = 0;
        float height = totalAvailableArea.Height;

        float offset = 0;
        Rectangle[] areas = [
            ..columnWidths
                .Select(width => {
                    Rectangle r = new(x + offset, 0, width, height);
                    offset += width;
                    return r;
                })
        ];

        return areas;
    }

    public static (float offset, float width) CalculateCellRegion(this GridLayout grid, GridPosition gridPosition)
    {
        float offset = grid.CalculateCellXOffset(gridPosition);
        float width = grid.CalculateCellWidth(gridPosition);
        return (offset, width);
    }

    public static float CalculateCellWidth(this GridLayout grid, GridPosition gridPosition) =>
        grid.Columns
            .Skip(gridPosition.Column)
            .Take(gridPosition.ColumnSpan)
            .Sum();

    public static float CalculateCellAvailableHeight(this GridLayout grid, GridPosition gridPosition) =>
        grid.Rows
            .Where(r => gridPosition.ContainsRowIndex(r.Row))
            .Select(r => r.Height)
            .Sum();

    private static float CalculateCellXOffset(this GridLayout grid, GridPosition gridPosition) =>
        grid.Columns
            .Take(gridPosition.Column - 1)
            .Sum();

    private static float TotalGridWidth(this GridLayout grid) =>
        grid.Columns.Sum();

    private static float[] RecalculateWidthsToFitIn(this GridLayout grid, float maxWidth)
    {
        float totalWidth = grid.TotalGridWidth();
        float aggregatedWidth = 0;
        float[] widths = [
            ..grid.Columns
            .Select((width, index) =>
            {
                float ratio = width / totalWidth;
                float calculatedWidth = index == grid.Columns.Length - 1
                    ? maxWidth - aggregatedWidth
                    : maxWidth * ratio;

                aggregatedWidth += calculatedWidth;
                return calculatedWidth;
            })
        ];

        return widths;
    }
}
