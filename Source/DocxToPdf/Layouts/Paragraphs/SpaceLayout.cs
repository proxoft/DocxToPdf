using Proxoft.DocxToPdf.Documents.Common;
using Proxoft.DocxToPdf.Documents.Shared;

namespace Proxoft.DocxToPdf.Layouts.Paragraphs;

internal record SpaceLayout(Rectangle BoundingBox, float BaselineOffset, Borders Borders) : ElementLayout(BoundingBox, Borders);
