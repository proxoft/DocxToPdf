using System;

namespace Proxoft.DocxToPdf.Documents.Styles.Paragraphs;

internal abstract class LineSpacing
{
    public abstract float CalculateSpaceAfterLine(float lineHeight);
}

internal sealed class AutoLineSpacing(long lineSpace) : LineSpacing
{
    public static readonly AutoLineSpacing Default = new(Unit);
    
    public const long Unit = 240;
    private readonly long _lineSpace = lineSpace;

    public override float CalculateSpaceAfterLine(float lineHeight)
    { 
        float spaceAfterLine = lineHeight * _lineSpace / Unit - lineHeight;
        return spaceAfterLine;
    }
}

internal sealed class ExactLineSpacing(float lineSpace) : LineSpacing
{
    private readonly float _lineSpace = lineSpace;

    public override float CalculateSpaceAfterLine(float lineHeight) => _lineSpace;
}

internal sealed class AtLeastLineSpacing(float lineSpace) : LineSpacing
{
    private readonly float _lineSpace = lineSpace;

    public override float CalculateSpaceAfterLine(float lineHeight)
    {
        return Math.Max(_lineSpace, lineHeight);
    }
}