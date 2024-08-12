using System.Diagnostics;
using UnityEngine.Assertions;

namespace pyr.Shared.Common;

public static class Assertions
{
    [Conditional("UNITY_ASSERTIONS")]
    public static void assert(bool condition, string message = null)
    {
        Assert.IsTrue(condition, message);
    }
}
