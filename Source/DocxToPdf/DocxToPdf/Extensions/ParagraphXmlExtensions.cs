using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Proxoft.DocxToPdf
{
    internal static class ParagraphXmlExtensions
    {
        public static IEnumerable<Run> SelectRuns(this Paragraph paragraph)
        {
            return paragraph
                .ChildElements
                .Where(c => c is Run || c is SdtRun)
                .SelectMany(child =>
                {
                    return child switch
                    {
                        Run r => new[] { r },
                        SdtRun sdtRun => sdtRun.SdtContentRun?.ChildElements.OfType<Run>() ?? Array.Empty<Run>(),
                        _ => Array.Empty<Run>()
                    };
                });
        }

        public static bool IsFieldStart(this Run run)
        {
            return run
                .Descendants<FieldChar>()
                .Where(fc => fc.FieldCharType != null && fc.FieldCharType == FieldCharValues.Begin)
                .Any();
        }

        public static bool IsFieldEnd(this Run run)
        {
            return run
                .Descendants<FieldChar>()
                .Where(fc => fc.FieldCharType != null && fc.FieldCharType == FieldCharValues.End)
                .Any();
        }
    }
}
