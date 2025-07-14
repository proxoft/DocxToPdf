using System.Diagnostics;
using Proxoft.DocxToPdf.Core;
using Proxoft.DocxToPdf.Core.Rendering;
using Proxoft.DocxToPdf.Core.Structs;
using Proxoft.DocxToPdf.Models.Common;
using Proxoft.DocxToPdf.Models.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs.Elements.Fields;

[DebuggerDisplay("{GetType().Name}:{_content}")]
internal abstract class Field(TextStyle textStyle) : LineElement
{
    private readonly TextStyle _textStyle = textStyle;
    private string _content = string.Empty;

    private Rectangle _lineRegion = Rectangle.Empty;

    public override double GetBaseLineOffset() =>
        _textStyle.CellAscent;

    public override sealed void Justify(DocumentPosition position, double baseLineOffset, Size lineSpace)
    {
        _lineRegion = new Rectangle(position.Offset, lineSpace);
        this.SetPosition(position);
    }

    public override void Render(IRendererPage page)
    {
        if (_textStyle.Background != System.Drawing.Color.Empty)
        {
            page.RenderRectangle(_lineRegion, _textStyle.Background);
        }

        Rectangle layout = new(this.Position.Offset, this.Size);
        page.RenderText(_content, _textStyle, layout);

        this.RenderBorder(page, page.Options.WordBorders);
    }

    public FieldUpdateResult Update(PageVariables variables)
    {
        this.UpdateCore(variables);

        double w = this.Width;
        _content = this.GetContent();
        this.Size = _textStyle.MeasureText(_content);

        return w < this.Size.Width
            ? FieldUpdateResult.NoChange
            : FieldUpdateResult.Resized;
    }

    protected abstract void UpdateCore(PageVariables variables);

    protected abstract string GetContent();
}
