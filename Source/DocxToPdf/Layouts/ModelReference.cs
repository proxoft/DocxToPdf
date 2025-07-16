using System.Linq;
using Proxoft.DocxToPdf.Documents;

namespace Proxoft.DocxToPdf.Layouts;

internal record ModelReference(ModelId[] Path)
{
    public static readonly ModelReference None = new([]);

    public bool IsNone => this.Path.Length == 0;

    public static ModelReference New(params ModelId[] items) =>
        new(items);

    public virtual bool Equals(ModelReference? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return this.Path.SequenceEqual(other.Path);
    }

    public override int GetHashCode()
    {
        // Aggregate hash code from array elements
        return this.Path.Aggregate(17, (hash, num) => hash * 31 + num.GetHashCode());
    }
}