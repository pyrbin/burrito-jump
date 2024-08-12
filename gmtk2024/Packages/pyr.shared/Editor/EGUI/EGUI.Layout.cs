using System;
using UnityEditor;
using UnityEngine;

namespace pyr.Shared.Editor.EGUI;

public static partial class EGUI
{
    public static void HorizontalLine(
        float height = 1,
        float width = -1,
        Vector2 margin = new()
    )
    {
        GUILayout.Space(margin.x);

        var rect = EditorGUILayout.GetControlRect(false, height);
        if (width > -1)
        {
            var centerX = rect.width / 2;
            rect.width = width;
            rect.x += centerX - width / 2;
        }

        var color = EditorStyles.label.active.textColor;
        color.a = 0.5f;
        EditorGUI.DrawRect(rect, color);

        GUILayout.Space(margin.y);
    }

    public static void Indent()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
    }

    public static void EndIndent()
    {
        GUILayout.Space(16);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    public readonly struct FoldoutScope : IDisposable
    {
        private readonly bool _WasIndent;

        public FoldoutScope(
            bool value,
            out bool shouldDraw,
            string label = "",
            bool indent = true,
            SerializedProperty? toggle = null,
            Action<Rect, bool>? drawLabel = null
        )
        {
            drawLabel ??= (rect, toggled) => { EditorGUI.LabelField(rect, label, EditorStyles.boldLabel); };

            value = Foldout(value, drawLabel, toggle);
            shouldDraw = value;
            if (shouldDraw && indent)
            {
                Indent();
                _WasIndent = true;
            }
            else
            {
                _WasIndent = false;
            }
        }

        private static bool Foldout(bool value, Action<Rect, bool> drawLabel, SerializedProperty? toggle = null)
        {
            bool _value;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            if (toggle is { boolValue: false })
            {
                EditorGUI.BeginDisabledGroup(true);
                _value = EditorGUILayout.Toggle(value, EditorStyles.foldout);
                EditorGUI.EndDisabledGroup();

                _value = false;
            }
            else
            {
                _value = EditorGUILayout.Toggle(value, EditorStyles.foldout);
            }

            if (toggle != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(toggle, GUIContent.none, GUILayout.Width(20));
                if (EditorGUI.EndChangeCheck() && toggle.boolValue)
                    _value = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            var rect = GUILayoutUtility.GetLastRect();
            rect.x += 20;
            rect.width -= 20;

            if (toggle is { boolValue: false })
            {
                EditorGUI.BeginDisabledGroup(true);
                drawLabel(rect, toggle.boolValue);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                drawLabel(rect, toggle?.boolValue ?? true);
            }

            return _value;
        }

        public void Dispose()
        {
            if (_WasIndent)
                EndIndent();
        }
    }
}
