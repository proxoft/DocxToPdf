using System.Linq;
using Proxoft.DocxToPdf.Builders.OpenXmlExtensions.Common;
using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Headers;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Builders.Headers;

internal static class HeaderBuilder
{
    public static Header Create(this Word.Header? header, BuilderServices services)
    {
        if(header is null)
        {
            return Header.None;
        }

        Model[] elements = [.. header.ParagraphsAndTables().Select(e => e.ToParagraphOrTable(services))];
        return new Header(
            services.IdFactory.NextHeaderId(),
            elements,
            Borders.None
        );
    }
}
