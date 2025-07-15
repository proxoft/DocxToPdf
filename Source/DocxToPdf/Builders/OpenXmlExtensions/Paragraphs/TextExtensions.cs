using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Documents.Paragraphs;

namespace Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;

internal static class TextExtensions
{
    public static Element[] SplitToElements(this Word.Text text, BuilderServices services) =>
        [..text.InnerText
            .SplitToWordsAndWhitechars()
            .Select(s =>
            {
                Element e = s switch
                {
                    " " => new Space(services.IdFactory.NextWordId()),
                    _ => new Text(services.IdFactory.NextWordId(), s)
                };

                return e;
            })
        ];

    private static IEnumerable<string> SplitToWordsAndWhitechars(this string text)
    {
        int start = 0, index;

        while ((index = text.IndexOfAny([' ', '\t'], start)) != -1)
        {
            if (index - start > 0)
                yield return text[start..index];

            yield return text.Substring(index, 1);
            start = index + 1;
        }

        if (start < text.Length)
        {
            yield return text[start..];
        }
    }
}
