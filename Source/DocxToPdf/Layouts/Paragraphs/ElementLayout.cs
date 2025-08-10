using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal abstract record ElementLayout(
    Size Size,
    float BaselineOffset,
    Rectangle BoundingBox,
    float LineBaseLineOffset,
    Borders Borders,
    LayoutPartition Partition
) : Layout(BoundingBox, Borders, Partition)
{
    public abstract TextStyle GetTextStyle();
}