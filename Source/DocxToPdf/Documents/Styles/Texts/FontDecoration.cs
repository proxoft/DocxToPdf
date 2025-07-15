using System;

namespace Proxoft.DocxToPdf.Documents.Styles.Texts;

[Flags]
internal enum FontDecoration
{
    Regular       = 0x0,
    Bold          = 0x1,
    Italic        = 0x2,
    Underline     = 0x4,
    Strikethrough = 0x8,
}
