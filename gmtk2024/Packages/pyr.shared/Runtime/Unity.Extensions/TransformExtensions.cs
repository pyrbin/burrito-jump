using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace pyr.Shared.Extensions;

public static class TransformExtensions
{
    public static IEnumerable<Transform> EnumerateHierarchy(this Transform root)
    {
        var transformQueue = new Queue<Transform>();
        transformQueue.Enqueue(root);
        while (transformQueue.Count > 0)
        {
            var parentTransform = transformQueue.Dequeue();
            if (!parentTransform) continue;
            for (var i = 0; i < parentTransform.childCount; i++) transformQueue.Enqueue(parentTransform.GetChild(i));
            yield return parentTransform;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RotateTowards(this Transform self, float2 to, float mod = 1f)
    {
        var direction = math.normalize(to - ((float3)self.position).xy);
        var angle = math.atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var eulerAngles = Vector3.forward * angle;

        if (!(math.abs(self.localEulerAngles.z - eulerAngles.z) > 0.3f)) return true;
        self.localRotation = Quaternion.Lerp(self.localRotation, Quaternion.Euler(eulerAngles), Time.deltaTime * mod);
        return false;
    }
}
