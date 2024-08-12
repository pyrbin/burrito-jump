using Unity.CodeEditor;
using UnityEditor;

namespace pyr.Core.Editor;

public static class AutoRegenerateProjectFiles
{
    [MenuItem("Tools / pyr / 🔩 Regenerate Project Files", false, 0)]
    private static void RegenerateProjectFiles()
    {
        CodeEditor.CurrentEditor.SyncAll();
    }
}
