using System;
using System.Collections.Generic;
using System.Linq;

namespace pyr.Shared.Extensions;

public static class ListExtensions
{
    public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
    {
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        return list;
    }

    public static IList<T> Swap<T>(this IList<T> list, T objA, T objB)
    {
        var indexA = list.IndexOf(objA);
        var indexB = list.IndexOf(objB);

        list[indexA] = list[indexB];
        list[indexB] = objA;

        return list;
    }

    public static void Move<T>(this IList<T> list, int fromIndex, int toIndex)
    {
        if (0 > fromIndex || fromIndex >= list.Count)
            throw new ArgumentException("From index is invalid");
        if (0 > toIndex || toIndex >= list.Count)
            throw new ArgumentException("To index is invalid");

        if (fromIndex == toIndex) return;
        var i = 0;
        var tmp = list[fromIndex];
        if (fromIndex < toIndex)
            for (i = fromIndex; i < toIndex; i++)
                list[i] = list[i + 1];
        else
            for (i = fromIndex; i > toIndex; i--)
                list[i] = list[i - 1];

        list[toIndex] = tmp;
    }

    private static void CopyTo<T>(this IList<T> sourceList, IList<T> destinationList, int sourceIndex = 0,
        int destinationIndex = 0, int count = -1)
    {
        if (count == -1)
            count = sourceList.Count;

        for (var i = 0; i < count; i++)
            destinationList[destinationIndex + i] = sourceList[sourceIndex + i];
    }

    public static bool IsSorted<T>(this IList<T> src, bool descending = false) where T : IComparable
    {
        var comparer = Comparer<T>.Default;
        return IsSorted(src, comparer, descending);
    }

    public static bool IsSorted<T>(this IList<T> src, Comparison<T> comparison, bool descending = false)
    {
        var comparer = Comparer<T>.Create(comparison);
        return IsSorted(src, comparer, descending);
    }

    private static bool IsSorted<T>(this IList<T> src, Comparer<T> comparer, bool descending = false)
    {
        for (var i = 1; i < src.Count; i++)
        {
            // Returns 1 if y is greater; -1 if y is smaller; 0 if equal
            var comparison = comparer.Compare(src[i - 1], src[i]);
            switch (comparison)
            {
                case > 0 when !descending:
                case < 0 when descending:
                    return false;
            }
        }

        return true;
    }

    public static void SortStable<T, TKey>(this IList<T> list, Func<T, TKey> selector, bool descending = false)
        where TKey : IComparable<TKey>
    {
        IList<T> orderedList = descending ? list.OrderByDescending(selector).ToList() : list.OrderBy(selector).ToList();
        CopyTo(orderedList, list);
    }

    public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
    {
        if (list is List<T> ts)
        {
            ts.Sort(comparison);
        }
        else
        {
            var copy = new List<T>(list);
            copy.Sort(comparison);
            copy.CopyTo(list);
        }
    }

    public static void SortBy<T, TParam>(this IList<T> list, Func<T, TParam> sortFunc)
        where TParam : IComparable
    {
        var comparison = new Comparison<T>((x, y) => sortFunc(x).CompareTo(sortFunc(y)));
        list.Sort(comparison);
    }

    public static void SortByDescending<T, TParam>(this IList<T> list, Func<T, TParam> sortFunc)
        where TParam : IComparable
    {
        var comparison = new Comparison<T>((x, y) => sortFunc(y).CompareTo(sortFunc(x)));
        list.Sort(comparison);
    }

    public static T AtOrDefault<T>(this IReadOnlyList<T> list, int i, T defaultValue = default)
    {
        return list.HasIndex(i) ? list[i] : defaultValue;
    }

    public static int FindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
    {
        for (var i = 0; i < list.Count; ++i)
            if (predicate(list[i]))
                return i;

        return -1;
    }

    public static int IndexOf<T>(this IReadOnlyList<T> list, T elementToFind)
    {
        for (var i = 0; i < list.Count; ++i)
            if (Equals(list[i], elementToFind))
                return i;

        return -1;
    }

    public static T GetOrDefault<T>(this IList<T>? list, int index, T onMissing = default)
    {
        if (list == null || !list.HasIndex(index))
            return onMissing;

        return list[index];
    }
}
