using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Assertions;

namespace pyr.Core.Time;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public record struct Duration
    : IComparable,
        IEquatable<TimeSpan>,
        IComparable<TimeSpan>
{
    private const uint NanosPerSec = 1_000_000_000;
    private const uint NanosPerMilli = 1_000_000;
    private const uint NanosPerMicro = 1_000;
    private const ulong MillisPerSec = 1_000;
    private const ulong MicrosPerSec = 1_000_000;

    [SerializeField] private UInt64 _Secs;

    [SerializeField] private UInt32 _Nanos;

    public readonly float Seconds => _Secs + (float)_Nanos / NanosPerSec;

    public readonly u128 Millis =>
        (u128)_Secs * (u128)MillisPerSec + _Nanos / (u128)NanosPerMilli;

    public readonly u128 Micros =>
        (u128)_Secs * (u128)MicrosPerSec + _Nanos / (u128)NanosPerMicro;

    public readonly u128 Nanos => _Nanos;

    public readonly bool IsZero()
    {
        return _Secs == 0 && _Nanos == 0;
    }

    public static Duration Zero => new(0, 0);
    public static Duration Sec => new(1, 0);
    public static Duration Milli => new(0, 1_000_000);
    public static Duration Micro => new(0, 1_000);
    public static Duration Nano => new(0, 1);
    public static Duration MaxValue => new(ulong.MaxValue, uint.MaxValue);
    public static Duration MinValue => new(ulong.MinValue, uint.MinValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Duration(ulong secs, uint nanos)
    {
        if (nanos < NanosPerSec)
        {
            _Secs = secs;
            _Nanos = nanos;
        }
        else
        {
            _Secs = checked(secs + nanos / NanosPerSec);
            _Nanos = nanos % NanosPerSec;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Duration(float secs)
    {
        Assert.IsTrue(secs >= 0.0);
        _Secs = (ulong)secs;
        _Nanos = (uint)((secs - _Secs) * NanosPerSec);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Duration(TimeSpan timeSpan)
        : this((ulong)timeSpan.Seconds, (uint)timeSpan.Milliseconds * NanosPerMilli)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Duration(ulong millis)
        : this(millis / MillisPerSec, (uint)(millis % MillisPerSec * NanosPerMilli))
    {
    }

    public static Duration FromSeconds(ulong secs)
    {
        return new Duration(secs, 0);
    }

    public static Duration FromSeconds(float secs)
    {
        return new Duration(secs);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration FromMillis(ulong millis)
    {
        return new Duration(millis / MillisPerSec, (uint)(millis % MillisPerSec * NanosPerMilli));
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration FromMicros(ulong micros)
    {
        return new Duration(micros / MicrosPerSec, (uint)(micros % MicrosPerSec * NanosPerMicro));
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration FromNanos(ulong nanos)
    {
        return new Duration(nanos / NanosPerSec, (uint)nanos % NanosPerSec);
    }

    public static explicit operator TimeSpan(Duration duration)
    {
        var ticks = (long)(
            duration._Secs * TimeSpan.TicksPerSecond + duration._Nanos / 100
        );
        return new TimeSpan(ticks);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Duration(TimeSpan timeSpan)
    {
        var secs = (ulong)timeSpan.TotalSeconds;
        var nanos = (uint)(timeSpan.Ticks % TimeSpan.TicksPerSecond * 100);
        return new Duration(secs, nanos);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Duration(float seconds)
    {
        return FromSeconds(seconds);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Duration Clamp(Duration min, Duration max)
    {
        return this < min ? min : this > max ? max : this;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(TimeSpan other)
    {
        return _Secs == (uint)other.Seconds && _Nanos == other.Milliseconds * NanosPerMilli;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int CompareTo(Duration other)
    {
        return _Secs != other._Secs
            ? _Secs < other._Secs ? -1 : 1
            : _Nanos < other._Nanos
                ? -1
                : _Nanos > other._Nanos
                    ? 1
                    : 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int CompareTo(TimeSpan other)
    {
        return _Secs != (uint)other.Seconds
            ? _Secs < (uint)other.Seconds ? -1 : 1
            : _Nanos < other.Milliseconds * NanosPerMilli
                ? -1
                : _Nanos > other.Milliseconds * NanosPerMilli
                    ? 1
                    : 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(object obj)
    {
        return obj switch
        {
            null => 1,
            Duration duration => CompareTo(duration),
            TimeSpan span => CompareTo(span),
            _ => throw new ArgumentException($"Object of type {obj.GetType()} cannot be compared to a Duration.")
        };
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator +(Duration lhs, Duration rhs)
    {
        return lhs.SaturatingAdd(rhs._Secs, rhs._Nanos);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator +(Duration lhs, TimeSpan rhs)
    {
        return lhs.SaturatingAdd((ulong)rhs.Seconds, (uint)rhs.Milliseconds * NanosPerMilli);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator -(Duration lhs, Duration rhs)
    {
        return lhs.SaturatingSub(rhs._Secs, rhs._Nanos);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator -(Duration lhs, TimeSpan rhs)
    {
        return lhs.SaturatingSub((ulong)rhs.Seconds, (uint)rhs.Milliseconds * NanosPerMilli);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator /(Duration lhs, Duration rhs)
    {
        return new Duration(lhs._Secs / rhs._Secs, lhs._Nanos / rhs._Nanos);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator /(Duration lhs, TimeSpan rhs)
    {
        return new Duration(lhs._Secs / (ulong)rhs.Seconds, lhs._Nanos / (uint)rhs.Milliseconds * NanosPerMilli);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator /(Duration lhs, float rhs)
    {
        return FromSeconds(lhs.Seconds / rhs);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration operator *(Duration lhs, float rhs)
    {
        return FromSeconds(lhs.Seconds * rhs);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Duration lhs, Duration rhs)
    {
        return lhs.CompareTo(rhs) >= 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Duration lhs, Duration rhs)
    {
        return lhs.CompareTo(rhs) <= 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Duration lhs, Duration rhs)
    {
        return lhs.CompareTo(rhs) > 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Duration lhs, Duration rhs)
    {
        return lhs.CompareTo(rhs) < 0;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Duration SaturatingAdd(ulong secs, uint nanos)
    {
        try
        {
            checked
            {
                var secsDelta = _Secs + secs;
                var nanosDelta = _Nanos + nanos;
                if (nanosDelta < NanosPerSec) return new Duration(secsDelta, nanosDelta);
                nanosDelta -= NanosPerSec;
                secsDelta += 1;

                return new Duration(secsDelta, nanosDelta);
            }
        }
        catch (OverflowException)
        {
            return MaxValue;
        }
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Duration SaturatingSub(ulong secs, uint nanos)
    {
        try
        {
            checked
            {
                var secsDelta = _Secs - secs;
                uint nanosDelta = 0;

                if (_Nanos >= nanos)
                {
                    nanosDelta = _Nanos - nanos;
                }
                else
                {
                    secsDelta -= 1;
                    nanosDelta = NanosPerSec + _Nanos - nanos;
                }

                return new Duration(secsDelta, nanosDelta);
            }
        }
        catch (OverflowException)
        {
            return MinValue;
        }
    }

    public override string ToString()
    {
        return $"{Seconds}s";
    }
}

public static class DurationLiteralExtensions
{
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration Secs(this float seconds)
    {
        return new Duration(seconds);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration Secs(this ulong seconds)
    {
        return new Duration(seconds, 0);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration Secs(this int seconds)
    {
        return new Duration((ulong)seconds, 0);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration Ms(this float millis)
    {
        return Duration.FromMillis((ulong)millis);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration Ms(this ulong millis)
    {
        return Duration.FromMillis(millis);
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Duration Ms(this int millis)
    {
        return Duration.FromMillis((ulong)millis);
    }
}
