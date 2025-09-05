using System.Collections.Generic;
using Proxoft.DocxToPdf.Documents.Sections;

namespace Proxoft.DocxToPdf.LayoutsBuilders.HeadersFooters;

internal static class SharedFunctions
{
    public static T FindForPage<T>(this Dictionary<PageNumberType, T> items, int pageNumber, bool hasTitlePage, bool useEvenOdd,  T none)
    {
        if(items.Count == 0) return none;
        if (pageNumber == 1) return items.FindForFirstPage(hasTitlePage, none);
        if (pageNumber % 2 == 0) return items.FindForEvenPage(useEvenOdd, none);
        return items.FindForOddPage(none);
    }

    private static T FindForFirstPage<T>(this Dictionary<PageNumberType, T> items, bool hasTitlePage, T none)
    {
        if (hasTitlePage && items.TryGetValue(PageNumberType.First, out T? value1)) return value1;
        if (items.TryGetValue(PageNumberType.Default, out T? value2)) return value2;
        return none;
    }

    private static T FindForOddPage<T>(this Dictionary<PageNumberType, T> items, T none)
    {
        if (items.TryGetValue(PageNumberType.Default, out T? value)) return value;
        return none;
    }

    private static T FindForEvenPage<T>(this Dictionary<PageNumberType, T> items, bool useEvenOdd, T none)
    {
        if (useEvenOdd && items.TryGetValue(PageNumberType.Even, out T? value1)) return value1;
        if (items.TryGetValue(PageNumberType.Default, out T? value2)) return value2;
        return none;
    }
}
