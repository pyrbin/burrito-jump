using System.Linq;
using UnityEditor;
using UnityEngine;

namespace pyr.Shared.Editor.EGUI;

public static partial class EGUI
{
    public static readonly float StandardHeightWithPadding
        = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

    public static bool DrawDefaultInspectorFiltered(
        SerializedObject obj,
        params string[] ignoreProperties
    )
    {
        EditorGUI.BeginChangeCheck();
        obj.Update();
        var iterator = obj.GetIterator();
        var enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            if (
                iterator.propertyPath == "m_Script"
                || ignoreProperties.Contains(iterator.propertyPath)
            )
                continue;

            EditorGUILayout.PropertyField(iterator, true);
            enterChildren = false;
        }

        obj.ApplyModifiedProperties();
        return EditorGUI.EndChangeCheck();
    }

    public static bool DrawProperty(SerializedObject obj, string propertyPath)
    {
        EditorGUI.BeginChangeCheck();
        obj.Update();

        var property = obj.FindProperty(propertyPath);
        EditorGUILayout.PropertyField(property, true);

        obj.ApplyModifiedProperties();
        return EditorGUI.EndChangeCheck();
    }

    public static bool DrawProperty(SerializedProperty property, ref Rect position, SerializedObject? obj = null)
    {
        EditorGUI.BeginChangeCheck();
        obj?.Update();
        position.height = EditorGUI.GetPropertyHeight(property);
        EditorGUI.PropertyField(position, property, true);
        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

        obj?.ApplyModifiedProperties();
        return EditorGUI.EndChangeCheck();
    }

    public static float GetHeightWithPadding(SerializedProperty property)
    {
        return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;
    }

    public static float CalcSize(string text, GUIStyle? style = null)
    {
        return (style ?? EditorStyles.label).CalcSize(new GUIContent(text)).x;
    }

    public static float CalcSize(GUIContent text, GUIStyle? style = null)
    {
        return (style ?? EditorStyles.label).CalcSize(text).x;
    }

    public static Texture2D CreateTexture(int width, int height, Color color)
    {
        var pixels = new Color[width * height];
        for (var i = 0; i < pixels.Length; i++) pixels[i] = color;

        var texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }
}
