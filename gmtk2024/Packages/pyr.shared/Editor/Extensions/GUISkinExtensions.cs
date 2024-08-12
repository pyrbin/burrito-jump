using UnityEngine;

namespace pyr.Shared.Editor.Extensions;

public static class GUISkinExtensions
{
    public static GUIStyle FindStyle(this GUISkin skin, string name, GUIStyle fallback)
    {
        return skin.FindStyle(name) ?? fallback;
    }

    public static GUIStyle FindStyle(this GUISkin skin, string name, string fallback)
    {
        return FindStyle(skin, name, (GUIStyle)fallback);
    }

    public static void Draw(this GUIStyle style, Rect rect)
    {
        if (Event.current.type == EventType.Repaint)
            style.Draw(rect, false, false, false, false);
    }
}
