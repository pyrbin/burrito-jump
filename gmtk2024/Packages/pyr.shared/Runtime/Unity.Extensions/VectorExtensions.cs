using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace pyr.Shared.Extensions;

public static unsafe class Int2Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int AsVector2Int(this int2 me)
    {
        return *(Vector2Int*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int AsVector3Int(this int2 me, int z = 0)
    {
        return new Vector3Int(me.x, me.y, z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int2 AsInt2(this Vector2Int me)
    {
        return *(int2*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int AsVector3Int(this int3 me)
    {
        return *(Vector3Int*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 AsInt3(this Vector3Int me)
    {
        return *(int3*)&me;
    }
}

public static unsafe class Float2Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 AsVector2(this float2 me)
    {
        return *(Vector2*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 AsFloat2(this Vector2 me)
    {
        return *(float2*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 x0y(this float2 me)
    {
        return new float3(me.x, 0, me.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Extend(this float2 me, float z)
    {
        return new float3(me.x, me.y, z);
    }
}

public static unsafe class Float3Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AsVector2(this float3 me)
    {
        return *(Vector3*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 AsFloat2(this Vector3 me)
    {
        return *(float3*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AsVector3(this float3 me)
    {
        return *(Vector3*)&me;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 AsFloat3(this Vector3 me)
    {
        return *(float3*)&me;
    }
}

public static class NumberExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToFloat(this int f)
    {
        return f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToFloat(this double f)
    {
        return (float)f;
    }
}
