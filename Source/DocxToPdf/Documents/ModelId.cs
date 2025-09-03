using System;
using System.Diagnostics;

namespace Proxoft.DocxToPdf.Documents;

[DebuggerDisplay("{_name}:{_id}")]
internal sealed class ModelId(string name, int id) : IEquatable<ModelId>
{
    public static readonly ModelId None = new("non", 0);

    private readonly string _name = name;
    private readonly int _id = id;

    public ModelId Next() =>
        new(_name, _id + 1);

    public override string ToString() =>
        $"{_name}:{_id}";

    public bool Equals(ModelId? other) =>
        other is not null && _name == other._name && _id == other._id;

    public override bool Equals(object? obj) =>
        obj is ModelId mid && this.Equals(mid);

    public override int GetHashCode() =>
        HashCode.Combine(_name, _id);

    public static bool operator ==(ModelId? left, ModelId? right) =>
        left is null && right is null || (left?.Equals(right) ?? false);

    public static bool operator !=(ModelId? left, ModelId? right) =>
        !(left == right);
}

internal sealed class ModelIdFactory
{
    private readonly ModelIdContainer _ignored = new("ign");

    private readonly ModelIdContainer _section = new("sct");
    private readonly ModelIdContainer _header = new("hdr");
    
    private readonly ModelIdContainer _paragraph = new("par");
    private readonly ModelIdContainer _word = new("wrd");
    private readonly ModelIdContainer _drawing = new("drw");

    private readonly ModelIdContainer _table = new("tbl");
    //private readonly ModelIdContainer _row = new("row");
    private readonly ModelIdContainer _cell = new("cel");

    public ModelId NextIgnoredId() =>
        _ignored.Next();

    public ModelId NextSectionId() =>
        _section.Next();

    public ModelId NextHeaderId() =>
        _header.Next();

    public ModelId NextParagraphId() =>
        _paragraph.Next();

    public ModelId NextWordId() =>
        _word.Next();

    public ModelId NextDrawingId() =>
        _drawing.Next();

    public ModelId NextTableId() =>
        _table.Next();

    public ModelId NextCellId() =>
        _cell.Next();

    private class ModelIdContainer(string name)
    {
        private ModelId _id = new(name, 0);

        public ModelId Next()
        {
            _id = _id.Next();
            return _id;
        }
    }
}

