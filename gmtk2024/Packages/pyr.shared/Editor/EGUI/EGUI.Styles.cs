using pyr.Shared.Extensions;
using UnityEditor;
using UnityEngine;

namespace pyr.Shared.Editor.EGUI;

public static partial class EGUI
{
    public const int TagPadding = 8;
    public static Color Yellow = "#fcc007".Rgba();
    public static Color DarkGrey = "#2a2a2a".Rgba();
    public static Color DarkestGrey = "#191919".Rgba();
    public static Color InspectorBackground = "#383838".Rgba();
    public static Color Green = "#1fae92".Rgba();
    public static Color Grey = new(0.7f, 0.7f, 0.7f, 0.5f);

    public static class Styles
    {
#pragma warning disable CS8618
        static Styles()
#pragma warning restore CS8618
        {
            InitializeStyles();
        }

        public static GUIStyle GreyLabel { get; private set; }
        public static GUIStyle YellowLabel { get; private set; }
        public static GUIStyle ValueBox { get; private set; }

        public static GUIStyle Tag(Color color, Color text)
        {
            return new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Normal,
                padding = new RectOffset(TagPadding / 2, TagPadding / 2, 0, 0),
                normal = { background = CreateTexture(2, 2, color), textColor = text }
            };
        }

        private static void InitializeStyles()
        {
            YellowLabel = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Yellow
                }
            };

            GreyLabel = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Grey
                }
            };

            ValueBox = new GUIStyle(EditorStyles.helpBox)
            {
                normal =
                {
                    textColor = Yellow
                }
            };
        }
    }
}
