using UnityEngine;

namespace pyr.Shared.Extensions;

public static class StringExtensions
{
    public static Color Hex(this string hex)
    {
        return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.red;
    }

    public static Color Rgba(this string rgba)
    {
        return ColorUtility.TryParseHtmlString(rgba, out var c) ? c : Color.red;
    }
}
