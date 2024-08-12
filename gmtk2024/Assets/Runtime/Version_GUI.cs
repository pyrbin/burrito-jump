namespace gmtk2024.Runtime
{
    [ExecuteAlways]
    public sealed class Version_GUI : MonoBehaviour
    {
        private Font? _Font;
        private GUIStyle? _Style;

        [ShowInInspector]
        private static string Version => Application.version;

        private void OnGUI()
        {
            _Style ??= new GUIStyle(GUI.skin.label);
            _Font ??= Resources.Load<Font>("iAWriterMonoV");

            _Style.fontSize = 16;
            _Style.padding = new RectOffset(7, 7, 0, 0);
            _Style.normal.textColor = Color.yellow;
            _Style.alignment = TextAnchor.LowerRight;
            _Style.font = _Font;

            var size = _Style.CalcSize(new GUIContent(Version));
            var x = 10;
            var y = Screen.height - size.y - 10;
            GUI.Label(new Rect(x, y, size.x, size.y), Version, _Style);
        }
    }
}
