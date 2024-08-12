using pyr.Shared.Editor.EGUI;
using UnityEditor;

namespace gmtk2024.Runtime.Stat.Editor;

[CustomPropertyDrawer(typeof(Resource), true)]
public class Resource_PropertyDrawer : Stat_PropertyDrawer
{
    protected const f32 CurrentSliderMaxWidth = 110f;
    protected Resource? Resource { get; set; }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);

        Resource = (Resource)Stat!;

        EditorGUI.BeginChangeCheck();
        var fractionLabel = ((Resource?.Fraction ?? 0) * 100).ToString("F0") + "%";
        var fractionLabelLength = EGUI.CalcSize(fractionLabel) + EGUI.TagPadding;
        var totalWidth = position.width;
        var sliderWidth = Mathf.Min(CurrentSliderMaxWidth, totalWidth - fractionLabelLength);

        var sliderValueRect = new Rect(LastPositionX + Padding, position.y, sliderWidth, Height);
        var fractionValueRect = new Rect(
            LastPositionX + sliderWidth + Padding * 2,
            position.y,
            fractionLabelLength,
            Height - .5f
        );

        var slider = EditorGUI.Slider(
            sliderValueRect,
            "",
            Resource?.Current?.Value ?? 0f,
            0f,
            Resource?.Value ?? 1f
        );

        EditorGUI.LabelField(
            fractionValueRect,
            ((Resource?.Fraction ?? 0) * 100).ToString("F0") + "%",
            EGUI.Styles.Tag(Color.black, EGUI.Yellow)
        );

        if (!EditorGUI.EndChangeCheck())
            return;
        if (Resource?.Current != null)
            Resource.Current.Value = slider;

        var targetObject = property.serializedObject.targetObject;
        EditorUtility.SetDirty(targetObject);
    }
}
