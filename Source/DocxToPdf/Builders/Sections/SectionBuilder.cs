using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Paragraphs;
using Proxoft.DocxToPdf.Builders.Paragraphs;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.Builders.Sections;

internal static class SectionBuilder
{
    public static Section[] ToSections(this Word.Body body, BuilderServices services) =>
        body.CreateSections(services);
}

file static class Operators
{
    public static Section[] CreateSections(this Word.Body body, BuilderServices services) =>
        [..body
            .SplitToSections()
            .Select(data => data.CreateSection(services))
        ];

    private static IEnumerable<SectionData> SplitToSections(this Word.Body body)
    {
        List<OpenXml.OpenXmlCompositeElement> sectionElements = [];

        foreach (OpenXml.OpenXmlCompositeElement child in body.RenderableChildren())
        {
            sectionElements.Add(child);

            if (child is not Word.Paragraph p)
            {
                continue;
            }

            Word.SectionProperties? secProps = p.GetSectionProperties();
            if (secProps is null)
            {
                continue;
            }

            yield return new SectionData(secProps, [.. sectionElements]);
            sectionElements.Clear();
        }

        Word.SectionProperties wordSectionProperties = body
           .ChildsOfType<Word.SectionProperties>()
           .Single();

        yield return new SectionData(null, [.. sectionElements]);
    }

    private static Section CreateSection(this SectionData data, BuilderServices services)
    {
        Model[] elements = [..data.Elements.Select(e => e.ToSectionChildModel(services))]; 
        return new Section(elements);
    }

    private static Model ToSectionChildModel(this OpenXml.OpenXmlCompositeElement element, BuilderServices services) =>
        element switch
        {
            Word.Paragraph p => p.ToParagraph(services),
            _ => new IgnoredModel(services.IdFactory.NextIgnoredId())
        };
}

file record SectionData(Word.SectionProperties? SectionProperties, OpenXml.OpenXmlCompositeElement[] Elements);


