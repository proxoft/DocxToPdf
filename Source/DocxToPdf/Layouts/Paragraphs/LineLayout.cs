using Proxoft.DocxToPdf.Documents.Common;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record LineLayout(
    ModelReference Source,
    Layout[] Childs,
    Rectangle BoundingBox
) : Layout(Source, BoundingBox)
{
    public static LineLayout New(ModelReference forParagraph) =>
        new(forParagraph, [], Rectangle.Empty);
}
