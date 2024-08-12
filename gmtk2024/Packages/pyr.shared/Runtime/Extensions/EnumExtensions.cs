using System;

namespace pyr.Shared.Extensions;

public static class EnumExtensions
{
    public static int GetIndex<T>(this T value) where T : Enum
    {
        return Array.IndexOf(Enum.GetValues(typeof(T)), value);
    }
}
