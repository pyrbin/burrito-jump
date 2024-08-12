#pragma warning disable CS8618

using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using pyr.Physics.Internal;
using pyr.Shared.Types;
using Unity.Mathematics;
using UnityEngine;

namespace pyr.Physics;

public static partial class Raycast
{
    private static OverlapSphereCommandAllScheduler OverlapSphereCommandAllScheduler;
    private static OverlapBoxCommandAllScheduler OverlapBoxCommandAllScheduler;
    private static OverlapCapsuleCommandAllScheduler OverlapCapsuleCommandAllScheduler;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UniTask<ColliderHit[]> Overlap(Shape shape, float3 origin = default, quaternion? quaternion = null,
        QueryParameters? queryParameters = null)
    {
#if UNITY_EDITOR
        if (!IsReady) return new UniTask<ColliderHit[]>(Array.Empty<ColliderHit>());
#endif
        var queryParametersResolved = queryParameters ?? QueryParameters.Default;
        var rotation = quaternion ?? Unity.Mathematics.quaternion.identity;

        return shape.Match
        (
            Sphere: radius => OverlapSphereCommandAllScheduler.Dispatch(origin, radius, queryParametersResolved),
            Box: extends => OverlapBoxCommandAllScheduler.Dispatch(origin, extends, rotation, queryParametersResolved),
            Capsule: (radius, height) => OverlapCapsuleCommandAllScheduler.Dispatch(origin,
                origin + math.mul(rotation, math.up()) * height, radius, queryParametersResolved)
        );
    }
}
