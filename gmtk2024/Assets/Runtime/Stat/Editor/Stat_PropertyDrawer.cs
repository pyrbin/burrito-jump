using System.Reflection;
using pyr.Shared.Editor.EGUI;
using UnityEditor;

namespace gmtk2024.Runtime.Stat.Editor;

[CustomPropertyDrawer(typeof(Stat), true)]
public class Stat_PropertyDrawer : PropertyDrawer
{
    protected const f32 Padding = 4f;
    protected const f32 PreflixLabelMinWidth = 90f;
    protected const f32 TagLabelMinWidth = 120f;
    protected const f32 ValueLabelMinWidth = 70f;

    protected f32 LastPositionX;
    protected f32 Height => EditorGUIUtility.singleLineHeight;

    protected Stat? Stat { get; set; }

    public override f32 GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return Height + EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var targetObject = property.serializedObject.targetObject;
        var targetType = targetObject.GetType();
        var field = targetType.GetField(
            property.propertyPath,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
        );
        Stat = (Stat?)(field?.GetValue(targetObject) ?? property.boxedValue);

        var typeName = Stat?.IconIdentifier + " " + Stat?.GetType().Name ?? "Unknown";
        var valueLabel = "= " + (Stat is null ? "N/A" : Stat?.Value.ToString("F2"));
        var labelLength =
            label.text.Length > 0 ? Mathf.Max(EGUI.CalcSize(label), PreflixLabelMinWidth) : 0;

        var prefixLabelRect = new Rect(position.x, position.y, labelLength, Height);

        EditorGUI.PrefixLabel(prefixLabelRect, GUIUtility.GetControlID(FocusType.Passive), label);

        position = new Rect(
            position.x + labelLength + Padding * 2,
            position.y,
            position.width - labelLength - Padding * 2,
            Height
        );

        EditorGUI.BeginProperty(position, label, property);

        var baseValueProp = property.FindPropertyRelative("_BaseValue");
        var typeNameLength = Mathf.Max(EGUI.CalcSize(typeName) + EGUI.TagPadding, TagLabelMinWidth);
        var valueLabelLength = Mathf.Max(
            EGUI.CalcSize(valueLabel) + EGUI.TagPadding,
            ValueLabelMinWidth
        );
        var baseValueLength = Mathf.Min(
            70f,
            position.width - typeNameLength - valueLabelLength - Padding * 3
        );

        var typeNameRect = new Rect(position.x + Padding, position.y, typeNameLength, Height - .5f);
        var valueRect = new Rect(
            position.x + typeNameLength + Padding * 2,
            position.y,
            valueLabelLength,
            Height - 0.5f
        );
        var baseValueRect = new Rect(
            position.x + typeNameLength + valueLabelLength + Padding * 3,
            position.y,
            baseValueLength,
            Height
        );

        EditorGUI.LabelField(
            typeNameRect,
            typeName,
            EGUI.Styles.Tag(EGUI.DarkestGrey, Color.white)
        );
        EditorGUI.LabelField(valueRect, valueLabel, EGUI.Styles.Tag(EGUI.DarkestGrey, EGUI.Yellow));
        EditorGUI.PropertyField(baseValueRect, baseValueProp, GUIContent.none);

        LastPositionX = baseValueRect.x + baseValueLength;

        const string baseLabel = "Base";
        var baseLabelLength = EGUI.CalcSize(baseLabel);
        baseValueRect.x += baseValueLength - baseLabelLength - Padding * 2;
        baseValueRect.y -= Padding / 2 - 0.5f;
        baseValueRect.width = baseLabelLength + Padding * 2;
        DrawPlaceholderText(baseValueRect, baseLabel);

        EditorGUI.EndProperty();
    }

    private static void DrawPlaceholderText(Rect rect, string placeholder)
    {
        var originalColor = GUI.color;
        GUI.color = EGUI.Grey;
        GUI.Label(rect, placeholder);
        GUI.color = originalColor;
    }
}
