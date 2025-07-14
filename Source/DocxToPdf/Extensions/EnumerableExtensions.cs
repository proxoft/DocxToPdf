using System;
using System.Collections.Generic;
using System.Linq;

namespace Proxoft.DocxToPdf.Extensions;

internal static class EnumerableExtensions
{
    public static T? FindPreviousOrDefault<T>(this IEnumerable<T> source, T item) where T : class =>
        source
            .Reverse()
            .SkipWhile(i => i != item)
            .Skip(1)
            .FirstOrDefault();

    public static T? FindNextOrDefault<T>(this IEnumerable<T> source, T item) where T : class =>
        source
            .SkipWhile(i => i != item)
            .Skip(1)
            .FirstOrDefault();

    public static IEnumerable<T> MergeAndFilter<T>(T value, IEnumerable<T> otherValues, Func<T, bool> predicate) =>
        new[] { value }
            .Concat(otherValues)
            .Where(v => predicate(v));

    public static Stack<T> ToStackReversed<T>(this IEnumerable<T> source) =>
        new(source.Reverse());

    //public static Stack<T> ToStack<T>(this IEnumerable<T> source, bool reverseOrder = true)
    //{
    //    var content = reverseOrder
    //        ? source.Reverse()
    //        : source;

    //    return new Stack<T>(content);
    //}

    public static void Push<T>(this Stack<T> stack, IEnumerable<T> items, bool reverseOrder = true)
    {
        var order = reverseOrder
            ? items.Reverse()
            : items;

        foreach (var item in order)
        {
            stack.Push(item);
        }
    }

    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int i = 0;
        foreach(var e in source)
        {
            if (predicate(e))
            {
                return i;
            }

            i++;
        }

        return -1;
    }
}
