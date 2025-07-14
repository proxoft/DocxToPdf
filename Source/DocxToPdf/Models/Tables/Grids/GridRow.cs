using System;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf.Models.Tables.Grids;

internal class GridRow(double height, HeightRuleValues heightRule)
{
    public double Height { get; private set; } = height;

    public HeightRuleValues HeightRule { get; } = heightRule;

    public void Expand(double height)
    {
        this.Height = Math.Max(height, this.Height);
    }
}
