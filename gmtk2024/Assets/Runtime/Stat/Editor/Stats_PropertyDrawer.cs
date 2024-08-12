using System.Reflection;
using ArteHacker.UITKEditorAid;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace gmtk2024.Runtime.Stat.Editor;

[CustomPropertyDrawer(typeof(Stats))]
public class Stats_PropertyDrawer : PropertyDrawer
{
    private SerializedProperty _Kvps;
    private ArrayPropertyField _ListField;
    private SerializedProperty _StatsMapProperty;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        FindProperties(property);

        var root = new VisualElement();

        _ListField = new ArrayPropertyField(
            _Kvps,
            i =>
            {
                var kvp = _Kvps.GetArrayElementAtIndex(i);
                var value = kvp.FindPropertyRelative("Value");
                return new PropertyField(value, string.Empty);
            }
        )
        {
            label = property.displayName,
            onAdd = DisplayAddMenu
        };

        UpdateListFieldAddButton();

        var sizeProp = _Kvps.FindPropertyRelative("Array.size");
        _ListField.Add(
            new ValueTracker<i32>(sizeProp, OnSizeChange, sizeProp.intValue)
            {
                name = "Size Tracker"
            }
        );

        root.Add(_ListField);

        return root;
    }

    private void OnSizeChange(ChangeEvent<i32> evt)
    {
        UpdateListFieldAddButton();
    }

    private void DisplayAddMenu(Rect buttonPosition)
    {
        var menu = new GenericMenu();

        foreach (
            var statTypeRecord in AvailableTypes(
                _StatsMapProperty.boxedValue as SerializedReferenceDictionary<StatType, Stat>
            )
        )
        {
            var statType = statTypeRecord.StatType;
            menu.AddItem(
                new GUIContent(
                    (
                        statTypeRecord.Type.GetCustomAttribute<IconIdentifierAttribute>()?.Icon
                        ?? "📈"
                    )
                        + " "
                        + statTypeRecord.Type.Name
                ),
                false,
                _ =>
                {
                    AddItemToArray(statTypeRecord);
                },
                statType
            );
        }

        menu.DropDown(buttonPosition);
    }

    private void AddItemToArray(StatTypeRecord statTypeRecord)
    {
        var newStat = Activator.CreateInstance(statTypeRecord.Type) as Stat;
        var key = statTypeRecord.StatType;

        _Kvps.arraySize++;

        var keyValueProp = _Kvps.GetArrayElementAtIndex(_Kvps.arraySize - 1);
        var keysProperty = keyValueProp.FindPropertyRelative("Key");
        var valuesProperty = keyValueProp.FindPropertyRelative("Value");

        keysProperty.enumValueIndex = key.GetIndex();
        valuesProperty.boxedValue = newStat;

        _Kvps.serializedObject.ApplyModifiedProperties();
        _StatsMapProperty.serializedObject.ApplyModifiedProperties();
    }

    private void UpdateListFieldAddButton()
    {
        _ListField.addButtonMode =
            AvailableTypes(
                _StatsMapProperty.boxedValue as SerializedReferenceDictionary<StatType, Stat>
            ).Count > 0
                ? AddButtonMode.WithOptions
                : AddButtonMode.None;
    }

    private void FindProperties(SerializedProperty property)
    {
        _StatsMapProperty = property.FindPropertyRelative("_Stats");
        _Kvps = _StatsMapProperty.FindPropertyRelative("_Kvp");
    }

    private static IReadOnlyCollection<StatTypeRecord> AvailableTypes(
        SerializedReferenceDictionary<StatType, Stat>? stats
    )
    {
        return TypeCache
            .GetTypesDerivedFrom<Stat>()
            .Where(x => !x.IsAbstract)
            .Select(x => new StatTypeRecord(x, x.ToStatType()))
            .Where(x =>
                !stats.Keys.Contains(x.StatType)
                && stats.Values?.FirstOrDefault(s => s?.GetType() == x.Type) is null
            )
            .ToArray();
    }

    private record struct StatTypeRecord(Type Type, StatType StatType);
}
