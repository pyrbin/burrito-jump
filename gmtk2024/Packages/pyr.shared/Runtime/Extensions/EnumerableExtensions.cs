using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace pyr.Shared.Extensions;

public static class EnumerableExtensions
{
    public static string Join<T>(this IEnumerable<T> enumerable, string separator)
    {
        return string.Join(separator, enumerable);
    }

    public static bool IsNullOrEmpty<T>(this ICollection<T>? enumerable)
    {
        return enumerable == null || enumerable.Count == 0;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable == null || !enumerable.Any();
    }

    public static IEnumerable Enumerate(this IEnumerator enumerator)
    {
        yield return enumerator.Current;
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }

    public static IEnumerable<T> Enumerate<T>(this IEnumerator<T> enumerator)
    {
        yield return enumerator.Current;
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }

    public static IEnumerable<T> Flatten<T, TKey, TValue>(this IDictionary<TKey, TValue> source,
        Func<KeyValuePair<TKey, TValue>, IEnumerable<T>> flattenFunc)
    {
        return source.SelectMany(flattenFunc.Invoke);
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source,
        Func<IEnumerable<T>, IEnumerable<T>>? flattenFunc = null)
    {
        return flattenFunc != null ? source.SelectMany(flattenFunc.Invoke) : source.SelectMany(x => x);
    }

    public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector,
        bool ascending)
    {
        return ascending ? enumerable.OrderBy(selector) : enumerable.OrderByDescending(selector);
    }

    public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> enumerable, Func<T, TKey> selector,
        bool ascending)
    {
        return ascending ? enumerable.ThenBy(selector) : enumerable.ThenByDescending(selector);
    }

    public static bool HasIndex<T>(this IEnumerable<T> list, int i)
    {
        return list != null && i >= 0 && i < list.Count();
    }

    public static bool HasIndex<T>(this IReadOnlyCollection<T> list, int i)
    {
        return list != null && i >= 0 && i < list.Count;
    }

    public static int RemoveAll<T>(this ICollection<T> set, Func<T, bool> condition)
    {
        return set.RemoveRange(set.Where(condition));
    }

    public static int RemoveRange<T>(this ICollection<T> set, IEnumerable<T> toRemove)
    {
        return toRemove.ToArray().Count(set.Remove);
    }

    public static bool RemoveFirst<T>(this ICollection<T> set, Func<T, bool> condition) where T : struct
    {
        if (!set.Any(condition)) return false;
        var entry = set.First(condition);
        return set.Remove(entry);
    }

    public static bool AddUnique<T>(this ICollection<T> list, T entry)
    {
        if (list.Contains(entry))
            return false;
        list.Add(entry);
        return true;
    }

    public static void AddRange<T>(this ICollection<T> set, IEnumerable<T> collection)
    {
        var l = collection.ToArray();
        foreach (var entry in l)
            set.Add(entry);
    }

    public static bool ContainEqual<T>(this IEnumerable<T> set1, IEnumerable<T>? set2)
    {
        if (set2 == null)
            return set1.IsNullOrEmpty();

        var entriesFound = 0;
        var enumerable = set1 as T[] ?? set1.ToArray();

        foreach (var entry in set2)
        {
            if (!enumerable.Contains(entry))
                return false;

            ++entriesFound;
        }

        return entriesFound == enumerable.Count();
    }

    public static bool ContainEqualNonAlloc<T>(this ICollection<T> set1, IEnumerable<T>? set2)
    {
        if (set2 == null)
            return set1.IsNullOrEmpty();

        var entriesFound = 0;
        foreach (var entry in set2)
        {
            if (!set1.Contains(entry))
                return false;

            ++entriesFound;
        }

        return entriesFound == set1.Count;
    }

    public static bool ContainsAny<T>(this ICollection<T> set1, ICollection<T> set2)
    {
        return set1.Any(set2.Contains);
    }

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var element in source)
            if (seenKeys.Add(keySelector(element)))
                yield return element;
    }

    public static ILookup<TKey, TValue> ToLookupMany<TKey, TGrouping, TGroupingValue, TValue>(
        this ICollection<TGrouping> values, Func<TGroupingValue, TKey> keySelector,
        Func<TGrouping, TValue> valueSelector)
        where TGrouping : ICollection<TGroupingValue>
    {
        return values
            .SelectMany(group => group.Select(value => (group, value)))
            .ToLookup(x => keySelector(x.value), x => valueSelector(x.group));
    }

    public static ILookup<TKey, TSource> ToLookupMany<TKey, TSource>(this ICollection<TSource> values,
        Func<TSource, ICollection<TKey>> keySelector)
    {
        return values
            .Select(source => (source, keys: keySelector(source)))
            .SelectMany(x => x.keys.Select(y => (x.source, key: y)))
            .ToLookup(x => x.key, x => x.source);
    }
}
