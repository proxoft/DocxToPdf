using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Styles.Borders;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;

internal static class BorderValuesConverter
{
    private static readonly Dictionary<Word.BorderValues, LineStyle> _map = new() {
        { Word.BorderValues.Nil, LineStyle.None },
        { Word.BorderValues.None, LineStyle.None },
        { Word.BorderValues.Single, LineStyle.Solid },
        { Word.BorderValues.Double, LineStyle.Solid },
        { Word.BorderValues.Dashed, LineStyle.Dashed },
        { Word.BorderValues.DashSmallGap, LineStyle.Dashed },
        { Word.BorderValues.Dotted, LineStyle.Dotted },
        { Word.BorderValues.DotDash, LineStyle.DotDash },
        { Word.BorderValues.DotDotDash, LineStyle.DotDotDash },
        { Word.BorderValues.ThinThickSmallGap, LineStyle.Solid }
    };

    public static LineStyle ToLineStyle(this OpenXml.EnumValue<Word.BorderValues>? borderValue) =>
        borderValue?.Value.ToLineStyle() ?? LineStyle.Solid;

    private static LineStyle ToLineStyle(this Word.BorderValues borderValue) =>
        _map.TryGetValue(borderValue, out LineStyle lineStyle) ? lineStyle : LineStyle.Solid;
}
