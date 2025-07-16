namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Units;

internal static class PointConverter
{
    private const float _pointsPerMilimeter = 2.85f;
    private const float _presentationPointsPerInch = 96;
    private const float _pointsPerInch = 72;

    public static float PointsFromMilimeter(this float millimeter) =>
        millimeter * _pointsPerMilimeter;

    public static float PointsFromCentimeter(this float centimeter) =>
        (centimeter * 100).PointsFromMilimeter();

    public static float PointsFromPresentationPoints(this float points) =>
        points * (_pointsPerInch / _presentationPointsPerInch);
}
