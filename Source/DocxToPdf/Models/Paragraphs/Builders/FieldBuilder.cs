using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements;
using Proxoft.DocxToPdf.Models.Paragraphs.Elements.Fields;
using Proxoft.DocxToPdf.Models.Styles.Services;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Builders;

internal static class FieldBuilder
{
    public static LineElement CreateField(
        this ICollection<Word.Run> runs,
        IStyleFactory styleFactory)
    {
        TextStyle style = styleFactory.EffectiveTextStyle(runs.First().RunProperties);

        Word.Run run = runs
            .Skip(1)
            .First();

        Word.FieldCode fieldCode = run
            .ChildsOfType<Word.FieldCode>()
            .Single();

        string text = fieldCode.Text;
        Field field = text.CreateField(style);
        field.Update(PageVariables.Empty);
        return field;
    }

    private static Field CreateField(
        this string text,
        TextStyle style)
    {
        string[] items = text.Split("\\");
        if (items.Length == 0)
        {
            return new EmptyField(style);
        }

        return items[0].Trim() switch
        {
            "PAGE" => new PageNumberField(style),
            "NUMPAGES" => new TotalPagesField(style),
            _ => new EmptyField(style),
        };
    }
}
