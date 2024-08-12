using pyr.Shared.Extensions;
using pyr.Shared.Types;
using Unity.Burst;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace pyr.Shared.Common;

public static class DebugDraw
{
    private const int Segments = 20;
    private static readonly float[] CosCache = new float[Segments];
    private static readonly float[] SinCache = new float[Segments];

    static DebugDraw()
    {
        var arc = math.PI * 2.0f / Segments;
        for (var i = 0; i < Segments; i++)
        {
            CosCache[i] = math.cos(arc * i);
            SinCache[i] = math.sin(arc * i);
        }
    }

    // 3D functions
    [BurstDiscard]
    public static void Circle(in float3 center, in float3 normal, float radius, Color color, float duration = 0.05f)
    {
        CalculatePlaneVectorsFromNormal(normal, out var v1, out var v2);
        CircleInternal(center, v1, v2, radius, color, duration);
    }

    [BurstDiscard]
    public static void Sphere(in float3 center, float radius, Color? color = null)
    {
        Sphere(center, radius, color ?? Color.white);
    }

    [BurstDiscard]
    public static void Shape(Shape shape, float3 center, quaternion rotation = default, Color? color = null,
        float duration = 0.05f)
    {
        shape.Do
        (
            Sphere: radius => Sphere(center, radius, color ?? Color.red, duration),
            Box: extents => Box(center, extents, rotation, color ?? Color.red, duration),
            Capsule: (radius, height) =>
            {
                var point1 = center - math.mul(rotation, math.up()) * height * 0.5f;
                var point2 = center + math.mul(rotation, math.up()) * height * 0.5f;
                Capsule(point1, point2, radius, color ?? Color.red, duration);
            }
        );
    }

    [BurstDiscard]
    public static void Capsule(in float3 point1, in float3 point2, float radius, Color color, float duration = 0.05f)
    {
        var capsuleDirection = point2 - point1;
        var capsuleCenter = (point1 + point2) * 0.5f;
        var capsuleUp = math.normalize(math.cross(capsuleDirection, math.right())) * radius;
        var capsuleForward = math.normalize(math.cross(capsuleUp, capsuleDirection)) * radius;

        Line(point1 + capsuleUp, point2 + capsuleUp, color, duration);
        Line(point1 - capsuleUp, point2 - capsuleUp, color, duration);
        Line(point1 + capsuleForward, point2 + capsuleForward, color, duration);
        Line(point1 - capsuleForward, point2 - capsuleForward, color, duration);

        Sphere(point1, radius, color, duration);
        Sphere(point2, radius, color, duration);
    }

    [BurstDiscard]
    public static void Capsule(in float3 center, in float3 direction, float radius, float height, Color color, float duration = 0.05f)
    {
        var halfHeight = height * 0.5f;
        var point1 = center - direction * halfHeight;
        var point2 = center + direction * halfHeight;

        Capsule(point1, point2, radius, color, duration);
    }

    [BurstDiscard]
    public static void Box(in float3 center, in float3 halfSize, in quaternion rotation, Color color, float duration = 0.05f)
    {
        var corner1 = math.mul(rotation, new float3(-halfSize.x, -halfSize.y, -halfSize.z)) + center;
        var corner2 = math.mul(rotation, new float3(halfSize.x, -halfSize.y, -halfSize.z)) + center;
        var corner3 = math.mul(rotation, new float3(halfSize.x, -halfSize.y, halfSize.z)) + center;
        var corner4 = math.mul(rotation, new float3(-halfSize.x, -halfSize.y, halfSize.z)) + center;
        var corner5 = math.mul(rotation, new float3(-halfSize.x, halfSize.y, -halfSize.z)) + center;
        var corner6 = math.mul(rotation, new float3(halfSize.x, halfSize.y, -halfSize.z)) + center;
        var corner7 = math.mul(rotation, new float3(halfSize.x, halfSize.y, halfSize.z)) + center;
        var corner8 = math.mul(rotation, new float3(-halfSize.x, halfSize.y, halfSize.z)) + center;

        Debug.DrawLine(corner1, corner2, color, duration);
        Debug.DrawLine(corner2, corner3, color, duration);
        Debug.DrawLine(corner3, corner4, color, duration);
        Debug.DrawLine(corner4, corner1, color, duration);

        Debug.DrawLine(corner5, corner6, color, duration);
        Debug.DrawLine(corner6, corner7, color, duration);
        Debug.DrawLine(corner7, corner8, color, duration);
        Debug.DrawLine(corner8, corner5, color, duration);

        Debug.DrawLine(corner1, corner5, color, duration);
        Debug.DrawLine(corner2, corner6, color, duration);
        Debug.DrawLine(corner3, corner7, color, duration);
        Debug.DrawLine(corner4, corner8, color, duration);
    }

    [BurstDiscard]
    public static void Arrow(in float3 pos, float angle, Color color, float length = 0.1f, float tipSize = 0.25f,
        float width = 1f, float duration = 0.05f)
    {
        var angleRot = quaternion.AxisAngle(math.up(), angle);
        var dir = math.mul(angleRot, math.forward());
        Arrow(pos, dir, color, length, tipSize, width, duration);
    }

    [BurstDiscard]
    public static void Arrow(in float3 pos, in float3 direction, Color color, float length = 0.1f, float tipSize = 0.25f,
        float width = 1f, float duration = 0.05f)
    {
        var dir = math.normalize(direction);

        var sideLen = length - length * tipSize;
        var widthOffset = math.cross(dir, math.up()) * width;

        var baseLeft = pos + widthOffset * 0.3f;
        var baseRight = pos - widthOffset * 0.3f;
        var tip = pos + dir * length;
        var upCornerInRight = pos - widthOffset * 0.3f + dir * sideLen;
        var upCornerInLeft = pos + widthOffset * 0.3f + dir * sideLen;
        var upCornerOutRight = pos - widthOffset * 0.5f + dir * sideLen;
        var upCornerOutLeft = pos + widthOffset * 0.5f + dir * sideLen;

        Debug.DrawLine(baseLeft, baseRight, color, duration);
        Debug.DrawLine(baseRight, upCornerInRight, color, duration);
        Debug.DrawLine(upCornerInRight, upCornerOutRight, color, duration);
        Debug.DrawLine(upCornerOutRight, tip, color, duration);
        Debug.DrawLine(tip, upCornerOutLeft, color, duration);
        Debug.DrawLine(upCornerOutLeft, upCornerInLeft, color, duration);
        Debug.DrawLine(upCornerInLeft, baseLeft, color, duration);
    }

    // 2D functions
    [BurstDiscard]
    public static void Circle(in float3 center, float radius, Color color, float duration = 0.05f)
    {
        CalculatePlaneVectorsFromNormal(math.up(), out var v1, out var v2);
        CircleInternal(center, v1, v2, radius, color, duration);
    }

    [BurstDiscard]
    public static void Arrow(in float3 pos, in float2 direction, Color color, float length = 0.1f, float tipSize = 0.25f,
        float width = 1f, float duration = 0.05f)
    {
        var dir = new float3(direction.x, 0f, direction.y);
        Arrow(pos, dir, color, length, tipSize, width, duration);
    }

    // Internal functions
    [BurstDiscard]
    private static void CircleInternal(in float3 center, in float3 v1, in float3 v2, float radius, Color color,
        float duration = 0.05f)
    {
        var p1 = center + v1 * radius;
        for (var i = 0; i < Segments; i++)
        {
            var p2 = center + v1 * CosCache[i] * radius + v2 * SinCache[i] * radius;
            Debug.DrawLine(p1, p2, color, duration);
            p1 = p2;
        }
    }

    private static void CalculatePlaneVectorsFromNormal(in float3 normal, out float3 v1, out float3 v2)
    {
        if (math.abs(math.dot(normal, math.up())) < 0.99)
        {
            v1 = math.normalize(math.cross(math.up(), normal));
            v2 = math.cross(normal, v1);
        }
        else
        {
            v1 = math.normalize(math.cross(math.left(), normal));
            v2 = math.cross(normal, v1);
        }
    }

    [BurstDiscard]
    public static void Line(in float3 from, in float3 to, Color color, float duration = 0.05f)
    {
        Debug.DrawLine(from, to, color, duration);
    }

    [BurstDiscard]
    public static void Sphere(in float3 center, float radius, Color color, float duration = 0.05f)
    {
        CircleInternal(center, math.right(), math.up(), radius, color, duration);
        CircleInternal(center, math.forward(), math.up(), radius, color, duration);
        CircleInternal(center, math.right(), math.forward(), radius, color, duration);
    }

    [BurstDiscard]
    public static void Path(in float3[] points, float duration = 0.05f)
    {
        if (points.Length < 2) return;
        for (var i = 0; i < points.Length - 1; i++)
            Debug.DrawLine(points[i], points[i + 1], Color.blue);
        Debug.DrawLine(points[0], points[1], Color.red, duration);
    }
}
