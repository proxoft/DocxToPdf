using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;
using Proxoft.DocxToPdf.Documents.Styles.Texts;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record TotalPagesLayout(
    string Content,
    Size Size,
    float BaselineOffset,
    Rectangle BoundingBox,
    float LineBaseLineOffset,
    Borders Borders,
    TextStyle TextStyle,
    LayoutPartition Partition) : FieldLayout(Content, Size, BaselineOffset, BoundingBox, LineBaseLineOffset, Borders, TextStyle, Partition);
