using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Common;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Footers;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Builders.Footers;

internal static class FooterBuilder
{
    public static Footer Create(this Word.Footer? footer, BuilderServices services)
    {
        if (footer is null) return Footer.None;

        Model[] elements = [.. footer.ParagraphsAndTables().Select(e => e.ToParagraphOrTable(services))];
        return new Footer(
            services.IdFactory.NextFooterId(),
            elements,
            Borders.None
        );
    }
}
