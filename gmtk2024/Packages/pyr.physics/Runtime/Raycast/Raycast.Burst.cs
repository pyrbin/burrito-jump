#pragma warning disable CS8618

using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using pyr.Physics.Internal;
using pyr.Shared.Types;
using pyr.Union.Monads;
using Unity.Mathematics;
using UnityEngine;
using static pyr.Union.Global;

namespace pyr.Physics;

public static partial class Raycast
{
    private static SpherecastCommandSingleScheduler SpherecastCommandSingleScheduler;
    private static SpherecastCommandAllScheduler SpherecastCommandAllScheduler;

    private static BoxcastCommandSingleScheduler BoxcastCommandSingleScheduler;
    private static BoxcastCommandAllScheduler BoxcastCommandAllScheduler;

    private static CapsulecastCommandSingleScheduler CapsulecastCommandSingleScheduler;
    private static CapsulecastCommandAllScheduler CapsulecastCommandAllScheduler;

    private static RaycastCommandSingleScheduler RaycastCommandSingleScheduler;
    private static RaycastCommandAllScheduler RaycastCommandAllScheduler;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UniTask<Option<RaycastHit>> Cast(Shape shape, Parameters @params = default,
        QueryParameters? queryParameters = null)
    {
#if UNITY_EDITOR
        if (!IsReady) return new UniTask<Option<RaycastHit>>(None);
#endif
        var queryParametersResolved = queryParameters ?? QueryParameters.Default;
        return shape.Match
        (
            Sphere: radius => SpherecastCommandSingleScheduler.Dispatch(radius, @params.Direction,
                @params.Distance, @params.Origin, queryParametersResolved),
            Box: extends => BoxcastCommandSingleScheduler.Dispatch(@params.Origin, extends, @params.Direction,
                @params.Distance, @params.Orientation ?? quaternion.identity, queryParametersResolved),
            Capsule: (radius, height) => CapsulecastCommandSingleScheduler.Dispatch(@params.Origin,
                @params.Origin + @params.Direction * height, radius, @params.Direction, @params.Distance,
                queryParametersResolved)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UniTask<RaycastHit[]> CastAll(Shape shape, Parameters @params = default,
        QueryParameters? queryParameters = null)
    {
#if UNITY_EDITOR
        if (!IsReady) return new UniTask<RaycastHit[]>(Array.Empty<RaycastHit>());
#endif
        var queryParametersResolved = queryParameters ?? QueryParameters.Default;
        return shape.Match
        (
            Sphere: radius => SpherecastCommandAllScheduler.Dispatch(radius, @params.Direction,
                @params.Distance, @params.Origin, queryParametersResolved),
            Box: extends => BoxcastCommandAllScheduler.Dispatch(@params.Origin, extends, @params.Direction,
                @params.Distance, @params.Orientation ?? quaternion.identity, queryParametersResolved),
            Capsule: (radius, height) => CapsulecastCommandAllScheduler.Dispatch(@params.Origin,
                @params.Origin + @params.Direction * height, radius, @params.Direction, @params.Distance,
                queryParametersResolved)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UniTask<Option<RaycastHit>> Ray(Ray ray, float distance = float.PositiveInfinity,
        QueryParameters? queryParameters = null)
    {
#if UNITY_EDITOR
        if (!IsReady) return new UniTask<Option<RaycastHit>>(None);
#endif
        var queryParametersResolved = queryParameters ?? QueryParameters.Default;
        return RaycastCommandSingleScheduler.Dispatch(ray.origin, ray.direction, distance,
            queryParametersResolved);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UniTask<RaycastHit[]> RayAll(Ray ray, float distance = float.PositiveInfinity,
        QueryParameters? queryParameters = null)
    {
#if UNITY_EDITOR
        if (!IsReady) return new UniTask<RaycastHit[]>(Array.Empty<RaycastHit>());
#endif
        var queryParametersResolved = queryParameters ?? QueryParameters.Default;
        return RaycastCommandAllScheduler.Dispatch(ray.origin, ray.direction, distance,
            queryParametersResolved);
    }
}
