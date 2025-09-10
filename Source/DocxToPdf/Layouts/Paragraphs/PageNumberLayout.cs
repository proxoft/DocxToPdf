using Proxoft.DocxToPdf.Documents;
using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record PageNumberLayout(
    ModelId Id,
    string Content,
    Size Size,
    float BaselineOffset,
    Rectangle BoundingBox,
    float LineBaseLineOffset,
    Borders Borders,
    TextStyle TextStyle,
    LayoutPartition Partition) : FieldLayout(Id, Content, Size, BaselineOffset, BoundingBox, LineBaseLineOffset, Borders, TextStyle, Partition);
