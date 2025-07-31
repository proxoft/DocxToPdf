using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Styles;

internal static class StyleDefinitionPartExtenstions
{
    public static IEnumerable<StyleParagraphProperties> GetParagraphStyles(this StyleDefinitionsPart? styleDefinitionsPart, StringValue? paragraphStyleId)
    {
        if (string.IsNullOrWhiteSpace(paragraphStyleId?.Value))
        {
            yield break;
        }

        StringValue? styleId = paragraphStyleId;
        do
        {
            Style? style = styleDefinitionsPart.FindStyle(styleId);
            if (style?.StyleParagraphProperties != null)
            {
                yield return style.StyleParagraphProperties;
            }

            styleId = style?.BasedOn?.Val;
        } while (styleId != null);
    }

    public static Style? FindStyle(this StyleDefinitionsPart? styleDefinitionsPart, StringValue styleId) =>
        styleDefinitionsPart?
            .Styles?
            .OfType<Style>()
            .SingleOrDefault(s => s.StyleId == styleId);
}
