using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Common;
using Proxoft.DocxToPdf.Builders.Paragraphs;
using Proxoft.DocxToPdf.Builders.Tables;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Builders;

internal static class ModelBuilderExtensions
{
    public static Model[] CreateParagraphsAndTables(this OpenXml.OpenXmlElement parent, BuilderServices services) =>
        [.. parent.ParagraphsAndTables().Select(e => e.ToParagraphOrTable(services))];

    public static Model ToParagraphOrTable(this OpenXml.OpenXmlCompositeElement element, BuilderServices services) =>
        element switch
        {
            Word.Paragraph p => p.ToParagraph(services),
            Word.Table t => t.ToTable(services),
            _ => new IgnoredModel(services.IdFactory.NextIgnoredId())
        };
}
