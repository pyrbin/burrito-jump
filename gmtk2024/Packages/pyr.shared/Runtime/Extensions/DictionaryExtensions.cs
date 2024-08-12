using System;
using System.Collections.Generic;
using System.Linq;

namespace pyr.Shared.Extensions;

public static class DictionaryExtensions
{
    public static V GetOrAdd<K, V>(
        this Dictionary<K, V> dictionary,
        K key,
        Func<V> valueProvider
    )
        where K : notnull
    {
        if (dictionary.TryGetValue(key, out var value)) return value;

        value = valueProvider();
        dictionary.Add(key, value);
        return value;
    }

    public static V GetOrAdd<K, V>(
        this Dictionary<K, V> dictionary,
        K key,
        V value
    )
        where K : notnull
    {
        return GetOrAdd(dictionary, key, () => value);
    }

    public static V GetOrAdd<K, V>(this Dictionary<K, V> dictionary, K key)
        where K : notnull
        where V : new()
    {
        return GetOrAdd(dictionary, key, new V());
    }

    public static V GetOrDefault<K, V>(this IDictionary<K, V> dict, K key, V onMissing = default)
    {
        if (key == null)
            return onMissing;
        return dict.TryGetValue(key, out var value) ? value : onMissing;
    }

    public static V GetOrDefault<K, V>(this IDictionary<K, object> dict, K key, V onMissing = default)
    {
        var o = dict!?.GetOrDefault(key) ?? default;
        if (o == null)
            return onMissing;
        return (V)Convert.ChangeType(o, typeof(V));
    }

    public static bool ContainsNonDefault<K, V>(this IDictionary<K, V?> dict, K key, out V? value)
    {
        if (dict.IsNullOrEmpty())
        {
            value = default;
            return false;
        }

        if (dict.TryGetValue(key, out value))
            return !value.IsDefault();

        return false;
    }

    public static bool ContainsNonDefault<K, V>(this IDictionary<K, V> dict, K key)
    {
        if (dict.IsNullOrEmpty()) return false;

        if (dict.TryGetValue(key, out var value))
            return !value.IsDefault();

        return false;
    }

    public static void RemoveAll<K, V>(this IDictionary<K, V> dict, Func<K, V, bool> condition)
    {
        var keys = dict.Keys.ToList();
        foreach (var key in keys.Where(key => dict.ContainsKey(key) && condition(key, dict[key]))) dict.Remove(key);
    }
}
