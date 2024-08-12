using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace pyr.Shared.Extensions;

public static class ValueTypeExtensions
{
    public static bool IsDefault<T>(this T value)
    {
        if (typeof(T).IsClass)
            return value == null;
        return value != null && value.Equals(default(T));
    }

    public static bool IsBetween(this float number, float min, float max)
    {
        if (min > max)
            (min, max) = (max, min);
        return number > min && number < max;
    }

    public static bool IsBetween(this double number, double min, double max)
    {
        if (min > max)
            (min, max) = (max, min);
        return number > min && number < max;
    }

    public static IEnumerable<T> ToEnumerable<T>(this ITuple tuple)
    {
        for (var n = 0; n < tuple.Length; n++)
            yield return (T)tuple[n];
    }
}
