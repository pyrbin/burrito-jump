using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using pyr.Shared.Editor.Extensions;
using pyr.Union.Monads;
using SaintsField;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace pyr.Shared.Editor.Drawers;

public class UnionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var height = EditorGUIUtility.singleLineHeight + 2;
        var (target, _) = Target(property);
        var valueProperty = ValueProperty(property);
        var kindProperty = KindProperty(property);

        if (target.IsNull() || valueProperty.IsNull() || kindProperty.IsNull())
            return EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        var found = false;

        foreach (var (valueFieldProperty, (name, prop)) in VariantValues(kindProperty.enumValueIndex, target,
                     valueProperty))
        {
            found = true;
            height += EditorGUI.GetPropertyHeight(valueFieldProperty, new GUIContent(name), true) + 2;
        }

        if (found)
            height += 4;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var (target, fieldInfo) = Target(property);
        if (target.IsNull()) return;

        var kindProperty = KindProperty(property);
        var valueProperty = ValueProperty(property);

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        position.height = EditorGUIUtility.singleLineHeight;
        position = EditorGUI.PrefixLabel(
            position,
            GUIUtility.GetControlID(FocusType.Passive),
            label
        );

        var variantProps = VariantValues(kindProperty.enumValueIndex, target, valueProperty).ToArray();
        var anyVariantProps = variantProps.Any();

        var toggleBgRect = position with { width = 20 };
        var toggleRect = toggleBgRect with { width = 20, x = position.x + 3 };

        var isReadOnly = fieldInfo?.GetCustomAttribute<ReadOnlyAttribute>() is not null
                         || fieldInfo?.GetCustomAttribute<ShowInInspectorAttribute>() is not null;

        if (!anyVariantProps)
            EditorGUI.BeginDisabledGroup(true);

        EditorGUI.HelpBox(toggleBgRect, GUIContent.none);

        property.isExpanded = EditorGUI.Toggle(toggleRect, isReadOnly || (anyVariantProps && property.isExpanded),
            EditorStyles.foldout);

        var kindRect = position with { width = position.width - 22, x = position.x + 22 };

        if (!anyVariantProps)
            EditorGUI.EndDisabledGroup();

        EditorGUI.PropertyField(kindRect, kindProperty, new GUIContent());

        if (property.isExpanded && anyVariantProps)
        {
            var offset = EditorGUIUtility.singleLineHeight + 2;
            var valueFieldRect = position with { y = position.y + offset, x = kindRect.x, width = kindRect.width };
            var singleProp = variantProps.Length == 1;
            foreach (var (valueFieldProperty, (name, prop)) in variantProps)
            {
                const float k_ValueWidthFactor = 0.6f;
                const float k_LabelWidthFactor = 1f - k_ValueWidthFactor;

                var fieldHeight = EditorGUI.GetPropertyHeight(valueFieldProperty, label, true) + 2;
                var fieldRect =
                    singleProp
                        ? valueFieldRect
                        : valueFieldRect with
                        {
                            width = valueFieldRect.width * k_ValueWidthFactor,
                            x = valueFieldRect.x + valueFieldRect.width * k_LabelWidthFactor
                        };

                if (!singleProp)
                {
                    var labelRect = valueFieldRect with
                    {
                        width = valueFieldRect.width * k_LabelWidthFactor
                    };

                    EditorGUI.LabelField(labelRect, name);
                }

                EditorGUI.PropertyField(fieldRect, valueFieldProperty, GUIContent.none);

                valueFieldRect.y += fieldHeight + 2;
            }
        }

        EditorGUI.EndProperty();
    }

    private static SerializedProperty KindProperty(SerializedProperty property)
    {
        return property.FindAutoPropertyRelative("__Kind");
    }

    private static SerializedProperty ValueProperty(SerializedProperty property)
    {
        return property.FindAutoPropertyRelative("__Value");
    }

    private static (object?, FieldInfo?) Target(SerializedProperty property)
    {
        var targetObject = property.serializedObject.targetObject;
        var targetType = targetObject.GetType();
        var field = targetType.GetField(
            property.propertyPath,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
        );

        var target = field?.GetValue(targetObject) ?? property.boxedValue;
        return (target.IsNull() ? null : target, field);
    }

    private IEnumerable<(SerializedProperty, (string Name, string Prop))> VariantValues(int index, object target,
        SerializedProperty valueProperty)
    {
        var method = target.GetType().GetMethod("GetValuePropertyNames");
        if (method.IsNull()) yield break;
        var props = (IEnumerable<(string, string)>)method.Invoke(target, new object[] { index });
        foreach (var (name, prop) in props)
        {
            var valueFieldProperty = valueProperty.FindAutoPropertyRelative(prop);
            if (valueFieldProperty.IsNull())
                continue;
            yield return (valueFieldProperty, (name, prop));
        }
    }
}
