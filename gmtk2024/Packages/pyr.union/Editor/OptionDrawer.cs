using pyr.Shared.Editor.Drawers;
using pyr.Union.Monads;
using UnityEditor;

namespace pyr.Union.Editor;

[CustomPropertyDrawer(typeof(Option<>))]
public sealed class OptionDrawer : UnionDrawer
{
}
