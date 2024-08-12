#pragma warning disable IDE1006 // Naming Styles

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
[NativeContainerSupportsDeallocateOnJobCompletion]
[DebuggerTypeProxy(typeof(NativePerJobThreadFloatPtrDebugView))]
[DebuggerDisplay("Value = {Value}")]
[StructLayout(LayoutKind.Sequential)]
public unsafe struct NativePerJobThreadFloatPtr : IDisposable
{
    [NativeContainer]
    [NativeContainerIsAtomicWriteOnly]
    public struct Parallel
    {
        [NativeDisableUnsafePtrRestriction] internal readonly float* m_Buffer;

        [NativeSetThreadIndex] internal readonly int m_ThreadIndex;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;

        internal Parallel(float* value, AtomicSafetyHandle safety)
        {
            m_Buffer = value;
            m_ThreadIndex = 0;
            m_Safety = safety;
        }
#else
        internal Parallel(float* value)
        {
            m_Buffer = value;
            m_ThreadIndex = 0;
        }
#endif

        [WriteAccessRequired]
        public void Increment()
        {
            RequireWriteAccess();
            m_Buffer[FloatsPerCacheLine * m_ThreadIndex]++;
        }

        [WriteAccessRequired]
        public void Decrement()
        {
            RequireWriteAccess();
            m_Buffer[FloatsPerCacheLine * m_ThreadIndex]--;
        }

        [WriteAccessRequired]
        public void Add(float value)
        {
            RequireWriteAccess();
            m_Buffer[FloatsPerCacheLine * m_ThreadIndex] += value;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void RequireWriteAccess()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
        }
    }

    [NativeDisableUnsafePtrRestriction] internal float* m_Buffer;

    internal Allocator m_AllocatorLabel;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    private AtomicSafetyHandle m_Safety;

    [NativeSetClassTypeToNullOnSchedule] private DisposeSentinel m_DisposeSentinel;
#endif

    private const int FloatsPerCacheLine = JobsUtility.CacheLineSize / sizeof(float);

    public NativePerJobThreadFloatPtr(Allocator allocator, float initialValue = 0)
    {
        if (allocator <= Allocator.None)
            throw new ArgumentException(
                "Allocator must be Temp, TempJob or Persistent",
                "allocator"
            );

        m_Buffer = (float*)
            UnsafeUtility.Malloc(
                JobsUtility.CacheLineSize * JobsUtility.MaxJobThreadCount,
                UnsafeUtility.AlignOf<float>(),
                allocator
            );

        m_AllocatorLabel = allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
#if UNITY_2018_3_OR_NEWER
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
#else
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0);
#endif
#endif

        Value = initialValue;
    }

    public float Value
    {
        get
        {
            RequireReadAccess();
            float value = 0;
            for (var i = 0; i < JobsUtility.MaxJobThreadCount; ++i) value += m_Buffer[FloatsPerCacheLine * i];
            return value;
        }
        [WriteAccessRequired]
        set
        {
            RequireWriteAccess();
            *m_Buffer = value;
            for (var i = 1; i < JobsUtility.MaxJobThreadCount; ++i) m_Buffer[FloatsPerCacheLine * i] = 0;
        }
    }

    public Parallel GetParallel()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        var parallel = new Parallel(m_Buffer, m_Safety);
        AtomicSafetyHandle.UseSecondaryVersion(ref parallel.m_Safety);
#else
        Parallel parallel = new Parallel(m_Buffer);
#endif
        return parallel;
    }

    public bool IsCreated => m_Buffer != null;

    [WriteAccessRequired]
    public void Dispose()
    {
        RequireWriteAccess();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
#if UNITY_2018_3_OR_NEWER
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#else
        DisposeSentinel.Dispose(m_Safety, ref m_DisposeSentinel);
#endif
#endif

        UnsafeUtility.Free(m_Buffer, m_AllocatorLabel);
        m_Buffer = null;
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    public void TestUseOnlySetAllowReadAndWriteAccess(bool allowReadOrWriteAccess)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.SetAllowReadOrWriteAccess(m_Safety, allowReadOrWriteAccess);
#endif
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private void RequireReadAccess()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    private void RequireWriteAccess()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
    }
}

internal sealed class NativePerJobThreadFloatPtrDebugView
{
    private NativePerJobThreadFloatPtr m_Ptr;

    public NativePerJobThreadFloatPtrDebugView(NativePerJobThreadFloatPtr ptr)
    {
        m_Ptr = ptr;
    }

    public float Value => m_Ptr.Value;
}
