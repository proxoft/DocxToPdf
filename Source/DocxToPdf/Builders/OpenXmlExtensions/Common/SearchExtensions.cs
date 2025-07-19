using System.Collections.Generic;
using System.Linq;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Common;

internal static class SearchExtensions
{
    public static IEnumerable<OpenXml.OpenXmlCompositeElement> ParagraphsAndTables(this OpenXml.OpenXmlElement parent) =>
       parent
            .ChildElements
            .Where(c => c is Word.Paragraph || c is Word.Table || c is Word.SdtBlock)
            .SelectMany(c =>
            {
                return c switch
                {
                    Word.SdtBlock block => block.SdtContentBlock?.ChildElements.OfType<OpenXml.OpenXmlCompositeElement>().ToArray() ?? [],
                    _ => new[] { c }
                };
            })
            .Cast<OpenXml.OpenXmlCompositeElement>();

    public static IEnumerable<T> ChildsOfType<T>(this OpenXml.OpenXmlElement xmlElement) where T : OpenXml.OpenXmlElement =>
        xmlElement.ChildElements.OfType<T>();
}
