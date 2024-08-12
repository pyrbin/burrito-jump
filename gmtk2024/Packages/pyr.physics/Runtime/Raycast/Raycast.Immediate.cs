#pragma warning disable CS8618

using System.Linq;
using System.Runtime.CompilerServices;
using pyr.Shared.Extensions;
using pyr.Shared.Types;
using pyr.Union.Monads;
using Unity.Mathematics;
using UnityEngine;
using static pyr.Union.Global;

#if UNITY_EDITOR
using ImmediatePhysics = Nomnom.RaycastVisualization.VisualPhysics;

#else
using ImmediatePhysics = UnityEngine.Physics;
#endif

namespace pyr.Physics;

public static partial class Raycast
{
    private static RaycastHit[] RaycastHits;
    private static Collider[] OverlapHits;

    public static class Now
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<RaycastHit> Cast(Shape shape, Parameters @params = default, LayerMask? layer = null)
        {
            var hits = CastAll(shape, @params, layer);
            if (hits.Length <= 0) return None;
            var closestHit = hits[0];
            var closestDistance = closestHit.distance;
            for (var i = 1; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!(hit.distance < closestDistance)) continue;
                closestHit = hit;
                closestDistance = hit.distance;
            }

            return closestHit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RaycastHit[] CastAll(Shape shape, Parameters @params = default, LayerMask? layer = null)
        {
            var hits = shape.Match(
                Sphere: radius => ImmediatePhysics.SphereCastNonAlloc(@params.Origin, radius, @params.Direction,
                    RaycastHits,
                    @params.Distance, layer ?? k_DefaultLayerMask),
                Box: halfExtends => ImmediatePhysics.BoxCastNonAlloc(@params.Origin, halfExtends, @params.Direction,
                    RaycastHits, @params.Orientation ?? quaternion.identity, @params.Distance,
                    layer ?? k_DefaultLayerMask),
                Capsule: (radius, height) => ImmediatePhysics.CapsuleCastNonAlloc(
                    @params.Origin,
                    @params.Origin + @params.Direction * height,
                    radius,
                    @params.Direction, RaycastHits, @params.Distance, layer ?? k_DefaultLayerMask)
            );
            return RaycastHits[..hits].Where(x => x.HasValidCollider()).ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<RaycastHit> Ray(Ray ray, float distance = float.PositiveInfinity, LayerMask? layer = null)
        {
            var hits = RayAll(ray, distance, layer);
            if (hits.Length <= 0) return None;
            var closestHit = hits[0];
            var closestDistance = closestHit.distance;
            for (var i = 1; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!(hit.distance < closestDistance)) continue;
                closestHit = hit;
                closestDistance = hit.distance;
            }

            return closestHit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RaycastHit[] RayAll(Ray ray, float distance = float.PositiveInfinity, LayerMask? layer = null)
        {
            var hits = ImmediatePhysics.RaycastNonAlloc(ray, RaycastHits, distance, layer ?? k_DefaultLayerMask);
            return RaycastHits[..hits].Where(x => x.HasValidCollider()).ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Collider[] Overlap(Shape shape, float3 origin = default, quaternion quaternion = default,
            LayerMask? layer = null)
        {
            int layerMask = layer ?? k_DefaultLayerMask;

            var hits = shape.Match
            (
                Sphere: radius => ImmediatePhysics.OverlapSphereNonAlloc(origin, radius, OverlapHits, layerMask),
                Box: extends =>
                    ImmediatePhysics.OverlapBoxNonAlloc(origin, extends, OverlapHits, quaternion, layerMask),
                Capsule: (radius, height) => ImmediatePhysics.OverlapCapsuleNonAlloc(origin,
                    origin + math.mul(quaternion, math.up()) * height, radius, OverlapHits, layerMask)
            );

            return OverlapHits[..hits].Where(x => x.HasValidCollider()).ToArray();
        }
    }
}
