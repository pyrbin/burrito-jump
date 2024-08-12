using System;
using System.Collections.Generic;
using pyr.Core.Time;
using pyr.Shared.Extensions;
using UnityEditor;
using UnityEngine;

namespace pyr.Core.Editor.Drawers;

[CustomPropertyDrawer(typeof(Duration))]
public class DurationDrawer : PropertyDrawer
{
    private const float k_SizeRatio = 1f;

    private static readonly TimeUnit[] DisplayUnits = { TimeUnit.Seconds, TimeUnit.Millis };
    private static readonly Dictionary<string, int> CachedDisplayUnitsIndex = new();

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        rect.width *= k_SizeRatio;

        rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

        EditorGUI.BeginProperty(rect, label, property);
        var id =
            $"{property.serializedObject.targetObject.GetInstanceID()}.{property.propertyPath}";
        var duration = (Duration?)property.boxedValue ?? Duration.Zero;
        var selectedIndex = CachedDisplayUnitsIndex.GetOrAdd(id, 0);

        const int k_TimeUnitWidth = 75;
        const int k_Padding = 4;

        Rect fieldRect = new(rect.x, rect.y, rect.width - k_TimeUnitWidth - k_Padding, rect.height);
        Rect unitRect =
            new(
                rect.x + rect.width - k_TimeUnitWidth + k_Padding,
                rect.y,
                k_TimeUnitWidth - k_Padding,
                rect.height
            );

        EditorGUI.BeginChangeCheck();

        for (var i = 0; i < DisplayUnits.Length; i++)
        {
            var itemRect = SplitRectWidth(unitRect, DisplayUnits.Length, i);
            var itemStyle = GetButtonStyle(DisplayUnits.Length, i);
            var itemName = FormatTimeUnit(DisplayUnits[i]);
            if (GUI.Toggle(itemRect, selectedIndex == i, itemName, itemStyle))
                selectedIndex = i;
        }

        var selectedUnit = DisplayUnits[selectedIndex];
        switch (selectedUnit)
        {
            case TimeUnit.Millis:
                duration = Duration.FromMillis(
                    (uint)EditorGUI.LongField(fieldRect, (long)duration.Millis)
                );
                fieldRect.x = fieldRect.x + fieldRect.width - 25f;
                DrawPlaceholderText(fieldRect, "ms");
                break;
            case TimeUnit.Seconds:
                duration = Duration.FromSeconds(EditorGUI.FloatField(fieldRect, duration.Seconds));
                fieldRect.x = fieldRect.x + fieldRect.width - 14f;
                DrawPlaceholderText(fieldRect, "s");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        CachedDisplayUnitsIndex[id] = selectedIndex;
        property.boxedValue = duration;
    }

    private static void DrawPlaceholderText(Rect rect, string placeholder)
    {
        var originalColor = GUI.color;
        GUI.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        GUI.Label(rect, placeholder);
        GUI.color = originalColor;
    }

    private static GUIStyle GetButtonStyle(int total, int current)
    {
        if (total <= 1)
            return EditorStyles.miniButton;

        if (current == 0)
            return EditorStyles.miniButtonLeft;

        return current == total - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid;
    }

    private static Rect SplitRectWidth(Rect rect, int total, int current)
    {
        if (total == 0)
            return rect;

        rect.width /= total;
        rect.x += rect.width * current;
        return rect;
    }

    private static string FormatTimeUnit(TimeUnit unit)
    {
        return unit switch
        {
            TimeUnit.Millis => "ms",
            TimeUnit.Seconds => "s",
            _ => throw new NotImplementedException()
        };
    }

    private enum TimeUnit
    {
        Millis,
        Seconds
    }
}
