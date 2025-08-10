using System.Globalization;

namespace Proxoft.DocxToPdf.Documents.Styles;

internal record Color(string Hex)
{
    public static readonly Color Empty = new("");
    public static readonly Color Black = new("000000");

    public (int r, int g, int b) Rgb => this.Hex == "auto"
        ? (0 , 0, 0) // black
        : this.Hex.ToRgb();
}


file static class ColorOperations
{
    public static (int r, int g, int b) ToRgb(this string hex)
    {
        if(hex == "")
        {
            return (-1, -1, -1);
        }

        int r = int.Parse(hex[..2], NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        return (r, g, b);
    }
}
