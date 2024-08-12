using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

namespace gmtk2024.Runtime.Renderer.Pixelate;

public static class PixelSnap
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 RoundToPixel(f32 unitsPerPixel, float3 position)
    {
        if (unitsPerPixel == 0.0f)
            return position;
        return math.round(position / unitsPerPixel) * unitsPerPixel;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Snap(
        quaternion cameraRotation,
        f32 unitsPerPixel,
        Transform target,
        out float3 displacement
    )
    {
        var alignedPos = math.mul(math.inverse(cameraRotation), target.position);
        var snappedPos = RoundToPixel(unitsPerPixel, alignedPos);
        displacement = alignedPos - snappedPos;
        target.position = math.mul(cameraRotation, snappedPos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Snap(quaternion cameraRotation, f32 unitsPerPixel, TransformAccess target)
    {
        var alignedPos = math.mul(math.inverse(cameraRotation), target.position);
        var snappedPos = RoundToPixel(unitsPerPixel, alignedPos);
        target.position = math.mul(cameraRotation, snappedPos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SnapAngle(i32 resolution, Transform target, out float3 displacement)
    {
        var angles = target.rotation.eulerAngles;
        var snapped = RoundToPixel(resolution, angles);
        target.rotation = quaternion.Euler(snapped);
        displacement = (float3)angles - snapped;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SnapAngle(i32 resolution, TransformAccess target)
    {
        var angles = target.rotation.eulerAngles;
        var snapped = RoundToPixel(resolution, angles);
        target.rotation = quaternion.Euler(snapped);
    }

    [BurstCompile]
    internal struct SnapTransformsJob : IJobParallelForTransform
    {
        public quaternion CameraRotation;
        public f32 UnitsPerPixel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(i32 index, TransformAccess transform)
        {
            Snap(CameraRotation, UnitsPerPixel, transform);
        }
    }

    [BurstCompile]
    internal struct UnsnapTransformsJob : IJobParallelForTransform
    {
        [Unity.Collections.ReadOnly]
        public NativeArray<float3> Positions;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(i32 index, TransformAccess transform)
        {
            transform.position = Positions[index];
        }
    }
}
