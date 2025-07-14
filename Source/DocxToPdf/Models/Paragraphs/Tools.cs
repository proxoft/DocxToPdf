using System;
using System.Collections.Generic;
using System.Linq;
using Proxoft.DocxToPdf.Extensions;
using Proxoft.DocxToPdf.Models.Core;

namespace Proxoft.DocxToPdf.Models.Paragraphs;

internal static class Tools
{
    public static double CalculateSpaceAfter(this PageContextElement element, IReadOnlyCollection<PageContextElement> allElements)
    {
        int index = allElements.IndexOf(e => e == element);
        var spaceBetween = CalculateSpaceBetween(element, allElements.SkipWhile(e => e != element).Skip(1).FirstOrDefault());
        return spaceBetween;
    }

    public static double CalculateSpaceBetween(PageContextElement element, PageContextElement? adjacentElement)
    {
        if(adjacentElement is null)
        {
            return 0;
        }

        double spaceAfter = element.SpaceAfter();
        double spaceBefore = adjacentElement.SpaceBefore();

        return Math.Max(spaceAfter, spaceBefore);
    }

    private static double SpaceAfter(this PageContextElement elementBase)
    {
        return (elementBase as Paragraph)?.SpaceAfter ?? 0;
    }

    private static double SpaceBefore(this PageContextElement elementBase)
    {
        return (elementBase as Paragraph)?.SpaceBefore ?? 0;
    }
}
