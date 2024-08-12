using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using pyr.Shared.Extensions;
using SaintsField;
using SaintsField.Playa;
using UnityEngine;

namespace pyr.Core.Time;

[Serializable]
public enum TimerMode
{
    Once,
    Repeating
}

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public record struct Timer
{
    [SerializeField] private TimerMode _Mode;

    [SerializeField] private Duration _Duration;

    [SerializeField] private Duration _Elapsed;

    [SerializeField] private bool _Paused;

    [Header("Runtime")] [SerializeField] [ShowInInspector] [ReadOnly]
    private bool _Finished;

    [SerializeField] [ReadOnly] private UInt32 _TimesFinishedThisTick;

    public TimerMode Mode
    {
        readonly get => _Mode;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Mode != TimerMode.Repeating && value == TimerMode.Repeating && _Finished)
            {
                _Elapsed = 0;
                _Finished = JustFinished;
            }

            _Mode = value;
        }
    }

    public Duration Duration => _Duration;

    public Duration Elapsed
    {
        readonly get => _Elapsed;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            _Elapsed = value.Clamp(Duration.Zero, _Duration);
            if (Mathf.Approximately(Fract, 1.0f))
                _Finished = true;
        }
    }

    public readonly bool Finished => _Finished;

    public readonly bool Paused => _Paused;

    [ShowInInspector]
    public readonly float Fract => _Duration == Duration.Zero ? 1.0f : _Elapsed.Seconds / _Duration.Seconds;

    [ShowInInspector] public float RemainingFract => 1.0f - Fract;

    public readonly float RemainingSeconds => Remaining.Seconds;

    [ShowInInspector] public readonly Duration Remaining => _Duration - _Elapsed;

    [ShowInInspector] public readonly float ElapsedSeconds => _Elapsed.Seconds;

    public readonly bool JustFinished => _TimesFinishedThisTick > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Timer()
        : this(Duration.Zero)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Timer(Duration duration, TimerMode mode = TimerMode.Once)
    {
        Mode = mode;
        _Duration = duration;
        _Elapsed = 0;
        _Paused = false;
        _Finished = _Elapsed >= _Duration;
        _TimesFinishedThisTick = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Timer FromSeconds(float seconds, TimerMode mode = TimerMode.Once)
    {
        return new Timer(Duration.FromSeconds(seconds), mode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Timer Tick(Duration delta)
    {
        if (_Paused)
        {
            _TimesFinishedThisTick = 0;
            if (Mode == TimerMode.Repeating) _Finished = false;
            return this;
        }

        if (Mode != TimerMode.Repeating && _Finished)
        {
            _TimesFinishedThisTick = 0;
            return this;
        }

        _Elapsed += delta;
        _Finished = _Elapsed >= _Duration;

        if (_Finished)
        {
            if (Mode == TimerMode.Repeating)
            {
                _TimesFinishedThisTick = _Duration.Nanos <= 0
                    ? uint.MaxValue
                    : _Elapsed
                        .Nanos.CheckedDiv(_Duration.Nanos)
                        .MapOr(uint.MaxValue, v => (uint)v);
                _Elapsed = _Elapsed
                    .Nanos.CheckedRem(_Duration.Nanos)
                    .MapOr(Duration.Zero, v => Duration.FromNanos((ulong)v));
            }
            else
            {
                _TimesFinishedThisTick = 1;
                _Elapsed = _Duration;
            }
        }
        else
        {
            _TimesFinishedThisTick = 0;
        }

        return this;
    }

    public void Pause()
    {
        _Paused = true;
    }

    public void Unpause()
    {
        _Paused = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        _Elapsed = 0;
        _Finished = false;
        _TimesFinishedThisTick = 0;
    }

    public readonly override string ToString()
    {
        return $"Timer: {_Elapsed} / {_Duration} ({Mode})";
    }
}
