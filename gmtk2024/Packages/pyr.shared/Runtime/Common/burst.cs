using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;

namespace pyr.Shared.Common;

public static class burst
{
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong umul128(ulong a, ulong b, out ulong low)
    {
        low = Unity.Burst.Intrinsics.Common.umul128(a, b, out var high);
        return high;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static u128 umul128(ulong a, ulong b)
    {
        var low = Unity.Burst.Intrinsics.Common.umul128(a, b, out var high);
        u128.Create(out var u, low, high);
        return u;
    }

    public static v128 neg(in this v128 u)
    {
        return X86.Sse2.IsSse2Supported ? X86.Sse2.xor_si128(u, new v128(0xFFFFFFFF)) : new v128(~u.ULong0, ~u.ULong1);
    }
}
