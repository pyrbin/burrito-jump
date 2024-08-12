using UnityEditor;

namespace gmtk2024.Runtime.Renderer.Pixelate.Editor;

[CustomEditor(typeof(PixelateFeature))]
public class PixelateRenderFeature_Inspector : UnityEditor.Editor
{
    internal PixelateFeature Target => (PixelateFeature)target;

    public override void OnInspectorGUI()
    {
        DrawCameraInspectorGroup();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        DrawDefaultInspectorGroup();
    }

    private void DrawDefaultInspectorGroup()
    {
        serializedObject.Update();
        var prop = serializedObject.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(false))
            EditorGUILayout.PropertyField(prop, true);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCameraInspectorGroup()
    {
        if (Target.Camera != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("_Camera", Target.Camera, typeof(Pixelate), true);

            var camProp = new SerializedObject(Target.Camera).GetIterator();
            camProp.NextVisible(true);
            while (camProp.NextVisible(false))
                EditorGUILayout.PropertyField(camProp, true);
            EditorGUI.EndDisabledGroup();
        }
    }
}
