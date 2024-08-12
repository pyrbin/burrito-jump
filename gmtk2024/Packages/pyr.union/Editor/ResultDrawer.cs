using pyr.Shared.Editor.Drawers;
using pyr.Union.Monads;
using UnityEditor;

namespace pyr.Union.Editor;

[CustomPropertyDrawer(typeof(Result<,>))]
public sealed class ResultDrawer : UnionDrawer
{
}
