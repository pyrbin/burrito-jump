#pragma warning disable CS8618

using System;
using LurkingNinja.PlayerloopManagement;
using pyr.Physics.Internal;
using pyr.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace pyr.Physics;

public static partial class Raycast
{
    private const int k_DefaultLayerMask = -5;

    private static bool IsReady;

    private static JobHandle Handle;

    private static void OnFixedUpdate(float dt)
    {
        if (!Handle.IsCompleted) return;

        if (!SpherecastCommandSingleScheduler.HasWork
            && !SpherecastCommandAllScheduler.HasWork
            && !BoxcastCommandSingleScheduler.HasWork
            && !BoxcastCommandAllScheduler.HasWork
            && !CapsulecastCommandSingleScheduler.HasWork
            && !CapsulecastCommandAllScheduler.HasWork
            && !RaycastCommandSingleScheduler.HasWork
            && !RaycastCommandAllScheduler.HasWork
            && !OverlapBoxCommandAllScheduler.HasWork
            && !OverlapCapsuleCommandAllScheduler.HasWork
            && !OverlapSphereCommandAllScheduler.HasWork) return;

        Handle.CombineDependencies
        (
            SpherecastCommandSingleScheduler.Schedule(),
            SpherecastCommandAllScheduler.Schedule(),
            BoxcastCommandSingleScheduler.Schedule(),
            BoxcastCommandAllScheduler.Schedule(),
            CapsulecastCommandSingleScheduler.Schedule(),
            CapsulecastCommandAllScheduler.Schedule(),
            RaycastCommandSingleScheduler.Schedule(),
            RaycastCommandAllScheduler.Schedule()
        );

        Handle.CombineDependencies
        (
            OverlapBoxCommandAllScheduler.Schedule(),
            OverlapCapsuleCommandAllScheduler.Schedule(),
            OverlapSphereCommandAllScheduler.Schedule()
        );

        Handle.Complete();

        SpherecastCommandSingleScheduler.Finish();
        SpherecastCommandAllScheduler.Finish();
        BoxcastCommandSingleScheduler.Finish();
        BoxcastCommandAllScheduler.Finish();
        CapsulecastCommandSingleScheduler.Finish();
        CapsulecastCommandAllScheduler.Finish();
        RaycastCommandSingleScheduler.Finish();
        RaycastCommandAllScheduler.Finish();
        OverlapBoxCommandAllScheduler.Finish();
        OverlapCapsuleCommandAllScheduler.Finish();
        OverlapSphereCommandAllScheduler.Finish();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void OnLoad()
    {
        Teardown();

        if (!Application.isPlaying)
            return;

        SpherecastCommandSingleScheduler = new SpherecastCommandSingleScheduler();
        SpherecastCommandAllScheduler = new SpherecastCommandAllScheduler();

        BoxcastCommandSingleScheduler = new BoxcastCommandSingleScheduler();
        BoxcastCommandAllScheduler = new BoxcastCommandAllScheduler();

        CapsulecastCommandSingleScheduler = new CapsulecastCommandSingleScheduler();
        CapsulecastCommandAllScheduler = new CapsulecastCommandAllScheduler();

        RaycastCommandSingleScheduler = new RaycastCommandSingleScheduler();
        RaycastCommandAllScheduler = new RaycastCommandAllScheduler();

        OverlapBoxCommandAllScheduler = new OverlapBoxCommandAllScheduler();
        OverlapCapsuleCommandAllScheduler = new OverlapCapsuleCommandAllScheduler();
        OverlapSphereCommandAllScheduler = new OverlapSphereCommandAllScheduler();

        RaycastHits = new RaycastHit[Constants.k_MaxHits];
        OverlapHits = new Collider[Constants.k_MaxHits];

        IsReady = true;

        Application.quitting += Teardown;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Teardown();
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += state =>
        {
            if (state is PlayModeStateChange.ExitingPlayMode) Teardown();
        };
#endif

        Playerloop.AddListener(new RaycastUpdateLoop());
    }

    private static void Teardown()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        SpherecastCommandSingleScheduler?.Dispose();
        SpherecastCommandAllScheduler?.Dispose();
        BoxcastCommandSingleScheduler?.Dispose();
        BoxcastCommandAllScheduler?.Dispose();
        CapsulecastCommandAllScheduler?.Dispose();
        CapsulecastCommandSingleScheduler?.Dispose();
        RaycastCommandAllScheduler?.Dispose();
        RaycastCommandSingleScheduler?.Dispose();

        IsReady = false;
    }

    [Serializable]
    public record struct Parameters(
        [field: SerializeField] float3 Origin = default,
        [field: SerializeField] float3 Direction = default,
        [field: SerializeField] float Distance = float.PositiveInfinity,
        [field: SerializeField] quaternion? Orientation = null
    );

    private struct RaycastHitEnumerator
    {
        private readonly NativeArray<RaycastHit> _Results;
        private readonly int _StartingIndex;

        private int _LocalIndex = 0;
        private readonly int _MaxHits;

        public RaycastHitEnumerator(ref NativeArray<RaycastHit> results, int raycastIndex, int maxHits)
        {
            _MaxHits = maxHits;
            _Results = results;
            _StartingIndex = raycastIndex * maxHits;
        }

        public bool HasNextHit(out RaycastHit hit)
        {
            if (_LocalIndex >= _MaxHits)
            {
                hit = default;
                return false;
            }

            var hitIndex = _StartingIndex + _LocalIndex;
            hit = _Results[hitIndex];
            if (hit.colliderInstanceID == 0) return false;

            ++_LocalIndex;
            return true;
        }
    }

    private struct OverlapHitEnumerator
    {
        private readonly NativeArray<ColliderHit> _Results;
        private readonly int _StartingIndex;

        private int _LocalIndex = 0;
        private readonly int _MaxHits;

        public OverlapHitEnumerator(ref NativeArray<ColliderHit> results, int raycastIndex, int maxHits)
        {
            _MaxHits = maxHits;
            _Results = results;
            _StartingIndex = raycastIndex * maxHits;
        }

        public bool HasNextHit(out ColliderHit hit)
        {
            if (_LocalIndex >= _MaxHits)
            {
                hit = default;
                return false;
            }

            var hitIndex = _StartingIndex + _LocalIndex;
            hit = _Results[hitIndex];
            if (hit.instanceID == 0) return false;

            ++_LocalIndex;
            return true;
        }
    }

    [BurstCompile]
    internal struct ResolveNearestHitJob : IJobFor
    {
        [ReadOnly] public NativeArray<RaycastHit> Results;

        [NativeDisableParallelForRestriction] public NativeArray<bool> HasHitResults;

        [NativeDisableParallelForRestriction] public NativeArray<RaycastHit> NearestHitResults;

        public int MaxHits;

        public void Execute(int index)
        {
            RaycastHitEnumerator hitEnumerator = new(ref Results, index, MaxHits);
            var nearestDistance = float.MaxValue;
            RaycastHit nearestHit = default;
            while (hitEnumerator.HasNextHit(out var hit))
            {
                HasHitResults[index] = true;

                var distance = hit.distance;
                if (distance > nearestDistance) continue;
                nearestDistance = distance;
                nearestHit = hit;
            }

            NearestHitResults[index] = nearestHit;
        }
    }

    private sealed class RaycastUpdateLoop : IFixedUpdate
    {
        public void OnFixedUpdate()
        {
            if (IsReady)
                Raycast.OnFixedUpdate(Time.deltaTime);
        }
    }
}
