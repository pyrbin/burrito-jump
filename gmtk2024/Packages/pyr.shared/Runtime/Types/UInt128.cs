// #FB_TODO: replace this with another implementation
// https://github.com/ricksladkey/dirichlet-numerics/blob/master/license.txt
// The MIT License (MIT)

// Copyright (c) .NET Foundation and Contributors

// All rights reserved.

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Globalization;
using System.Linq;
using System.Numerics;
using pyr.Shared.Common;
using static Unity.Mathematics.math;

namespace System;

public struct u128 : IFormattable, IComparable, IComparable<u128>, IEquatable<u128>
{
    private struct u256
    {
        public ulong s0;
        public ulong s1;
        public ulong s2;
        public ulong s3;

        public uint r0
        {
            get { return (uint)s0; }
        }
        public uint r1
        {
            get { return (uint)(s0 >> 32); }
        }
        public uint r2
        {
            get { return (uint)s1; }
        }
        public uint r3
        {
            get { return (uint)(s1 >> 32); }
        }
        public uint r4
        {
            get { return (uint)s2; }
        }
        public uint r5
        {
            get { return (uint)(s2 >> 32); }
        }
        public uint r6
        {
            get { return (uint)s3; }
        }
        public uint r7
        {
            get { return (uint)(s3 >> 32); }
        }

        public u128 t0
        {
            get
            {
                u128 result;
                Create(out result, s0, s1);
                return result;
            }
        }
        public u128 t1
        {
            get
            {
                u128 result;
                Create(out result, s2, s3);
                return result;
            }
        }

        public static implicit operator BigInteger(u256 a)
        {
            return (BigInteger)a.s3 << 192
                | (BigInteger)a.s2 << 128
                | (BigInteger)a.s1 << 64
                | a.s0;
        }

        public override string ToString()
        {
            return ((BigInteger)this).ToString();
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    private ulong s0;
    private ulong s1;
#pragma warning restore IDE1006 // Naming Styles

    private static readonly u128 maxValue = ~(u128)0;
    private static readonly u128 zero = (u128)0;
    private static readonly u128 one = (u128)1;

    public static u128 MinValue
    {
        get { return zero; }
    }
    public static u128 MaxValue
    {
        get { return maxValue; }
    }
    public static u128 Zero
    {
        get { return zero; }
    }
    public static u128 One
    {
        get { return one; }
    }

    public static u128 Parse(string value)
    {
        u128 c;
        if (!TryParse(value, out c))
            throw new FormatException();
        return c;
    }

    public static bool TryParse(string value, out u128 result)
    {
        return TryParse(value, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
    }

    public static bool TryParse(
        string value,
        NumberStyles style,
        IFormatProvider provider,
        out u128 result
    )
    {
        BigInteger a;
        if (!BigInteger.TryParse(value, style, provider, out a))
        {
            result = Zero;
            return false;
        }
        Create(out result, a);
        return true;
    }

    public u128(long value)
    {
        Create(out this, value);
    }

    public u128(ulong value)
    {
        Create(out this, value);
    }

    public u128(decimal value)
    {
        Create(out this, value);
    }

    public u128(double value)
    {
        Create(out this, value);
    }

    public u128(BigInteger value)
    {
        Create(out this, value);
    }

    public static void Create(out u128 c, uint r0, uint r1, uint r2, uint r3)
    {
        c.s0 = (ulong)r1 << 32 | r0;
        c.s1 = (ulong)r3 << 32 | r2;
    }

    public static void Create(out u128 c, ulong s0, ulong s1)
    {
        c.s0 = s0;
        c.s1 = s1;
    }

    public static void Create(out u128 c, long a)
    {
        c.s0 = (ulong)a;
        c.s1 = a < 0 ? ulong.MaxValue : 0;
    }

    public static void Create(out u128 c, ulong a)
    {
        c.s0 = a;
        c.s1 = 0;
    }

    public static void Create(out u128 c, decimal a)
    {
        var bits = decimal.GetBits(decimal.Truncate(a));
        Create(out c, (uint)bits[0], (uint)bits[1], (uint)bits[2], 0);
        if (a < 0)
            Negate(ref c);
    }

    public static void Create(out u128 c, BigInteger a)
    {
        var sign = a.Sign;
        if (sign == -1)
            a = -a;
        c.s0 = (ulong)(a & ulong.MaxValue);
        c.s1 = (ulong)(a >> 64);
        if (sign == -1)
            Negate(ref c);
    }

    public static void Create(out u128 c, double a)
    {
        var negate = false;
        if (a < 0)
        {
            negate = true;
            a = -a;
        }
        if (a <= ulong.MaxValue)
        {
            c.s0 = (ulong)a;
            c.s1 = 0;
        }
        else
        {
            var shift = Math.Max((int)ceil(Math.Log(a, 2)) - 63, 0);
            c.s0 = (ulong)(a / pow(2, shift));
            c.s1 = 0;
            LeftShift(ref c, shift);
        }
        if (negate)
            Negate(ref c);
    }

    private uint r0
    {
        get { return (uint)s0; }
    }
    private uint r1
    {
        get { return (uint)(s0 >> 32); }
    }
    private uint r2
    {
        get { return (uint)s1; }
    }
    private uint r3
    {
        get { return (uint)(s1 >> 32); }
    }

    public ulong S0
    {
        get { return s0; }
    }
    public ulong S1
    {
        get { return s1; }
    }

    public bool IsZero
    {
        get { return (s0 | s1) == 0; }
    }
    public bool IsOne
    {
        get { return s1 == 0 && s0 == 1; }
    }
    public bool IsPowerOfTwo
    {
        get { return (this & (this - 1)).IsZero; }
    }
    public bool IsEven
    {
        get { return (s0 & 1) == 0; }
    }
    public int Sign
    {
        get { return IsZero ? 0 : 1; }
    }

    public override string ToString()
    {
        return ((BigInteger)this).ToString();
    }

    public string ToString(string format)
    {
        return ((BigInteger)this).ToString(format);
    }

    public string ToString(IFormatProvider provider)
    {
        return ToString(null, provider);
    }

    public string ToString(string format, IFormatProvider provider)
    {
        return ((BigInteger)this).ToString(format, provider);
    }

    public static explicit operator u128(double a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static explicit operator u128(sbyte a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static implicit operator u128(byte a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static explicit operator u128(short a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static implicit operator u128(ushort a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static explicit operator u128(int a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static implicit operator u128(uint a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static explicit operator u128(long a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static implicit operator u128(ulong a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static explicit operator u128(decimal a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static explicit operator u128(BigInteger a)
    {
        u128 c;
        Create(out c, a);
        return c;
    }

    public static explicit operator float(u128 a)
    {
        return ConvertToFloat(ref a);
    }

    public static explicit operator double(u128 a)
    {
        return ConvertToDouble(ref a);
    }

    public static float ConvertToFloat(ref u128 a)
    {
        if (a.s1 == 0)
            return a.s0;
        return a.s1 * (float)ulong.MaxValue + a.s0;
    }

    public static double ConvertToDouble(ref u128 a)
    {
        if (a.s1 == 0)
            return a.s0;
        return a.s1 * (double)ulong.MaxValue + a.s0;
    }

    public static explicit operator sbyte(u128 a)
    {
        return (sbyte)a.s0;
    }

    public static explicit operator byte(u128 a)
    {
        return (byte)a.s0;
    }

    public static explicit operator short(u128 a)
    {
        return (short)a.s0;
    }

    public static explicit operator ushort(u128 a)
    {
        return (ushort)a.s0;
    }

    public static explicit operator int(u128 a)
    {
        return (int)a.s0;
    }

    public static explicit operator uint(u128 a)
    {
        return (uint)a.s0;
    }

    public static explicit operator long(u128 a)
    {
        return (long)a.s0;
    }

    public static explicit operator ulong(u128 a)
    {
        return a.s0;
    }

    public static explicit operator decimal(u128 a)
    {
        if (a.s1 == 0)
            return a.s0;
        var shift = Math.Max(0, 32 - GetBitLength(a.s1));
        u128 ashift;
        RightShift(out ashift, ref a, shift);
        return new decimal((int)a.r0, (int)a.r1, (int)a.r2, false, (byte)shift);
    }

    public static implicit operator BigInteger(u128 a)
    {
        if (a.s1 == 0)
            return a.s0;
        return (BigInteger)a.s1 << 64 | a.s0;
    }

    public static u128 operator <<(u128 a, int b)
    {
        u128 c;
        LeftShift(out c, ref a, b);
        return c;
    }

    public static u128 operator >>(u128 a, int b)
    {
        u128 c;
        RightShift(out c, ref a, b);
        return c;
    }

    public static u128 operator &(u128 a, u128 b)
    {
        u128 c;
        And(out c, ref a, ref b);
        return c;
    }

    public static uint operator &(u128 a, uint b)
    {
        return (uint)a.s0 & b;
    }

    public static uint operator &(uint a, u128 b)
    {
        return a & (uint)b.s0;
    }

    public static ulong operator &(u128 a, ulong b)
    {
        return a.s0 & b;
    }

    public static ulong operator &(ulong a, u128 b)
    {
        return a & b.s0;
    }

    public static u128 operator |(u128 a, u128 b)
    {
        u128 c;
        Or(out c, ref a, ref b);
        return c;
    }

    public static u128 operator ^(u128 a, u128 b)
    {
        u128 c;
        ExclusiveOr(out c, ref a, ref b);
        return c;
    }

    public static u128 operator ~(u128 a)
    {
        u128 c;
        Not(out c, ref a);
        return c;
    }

    public static u128 operator +(u128 a, u128 b)
    {
        u128 c;
        Add(out c, ref a, ref b);
        return c;
    }

    public static u128 operator +(u128 a, ulong b)
    {
        u128 c;
        Add(out c, ref a, b);
        return c;
    }

    public static u128 operator +(ulong a, u128 b)
    {
        u128 c;
        Add(out c, ref b, a);
        return c;
    }

    public static u128 operator ++(u128 a)
    {
        u128 c;
        Add(out c, ref a, 1);
        return c;
    }

    public static u128 operator -(u128 a, u128 b)
    {
        u128 c;
        Subtract(out c, ref a, ref b);
        return c;
    }

    public static u128 operator -(u128 a, ulong b)
    {
        u128 c;
        Subtract(out c, ref a, b);
        return c;
    }

    public static u128 operator -(ulong a, u128 b)
    {
        u128 c;
        Subtract(out c, a, ref b);
        return c;
    }

    public static u128 operator --(u128 a)
    {
        u128 c;
        Subtract(out c, ref a, 1);
        return c;
    }

    public static u128 operator +(u128 a)
    {
        return a;
    }

    public static u128 operator *(u128 a, uint b)
    {
        u128 c;
        Multiply(out c, ref a, b);
        return c;
    }

    public static u128 operator *(uint a, u128 b)
    {
        u128 c;
        Multiply(out c, ref b, a);
        return c;
    }

    public static u128 operator *(u128 a, ulong b)
    {
        u128 c;
        Multiply(out c, ref a, b);
        return c;
    }

    public static u128 operator *(ulong a, u128 b)
    {
        u128 c;
        Multiply(out c, ref b, a);
        return c;
    }

    public static u128 operator *(u128 a, u128 b)
    {
        u128 c;
        Multiply(out c, ref a, ref b);
        return c;
    }

    public static u128 operator /(u128 a, ulong b)
    {
        u128 c;
        Divide(out c, ref a, b);
        return c;
    }

    public static u128 operator /(u128 a, u128 b)
    {
        u128 c;
        Divide(out c, ref a, ref b);
        return c;
    }

    public static ulong operator %(u128 a, uint b)
    {
        return Remainder(ref a, b);
    }

    public static ulong operator %(u128 a, ulong b)
    {
        return Remainder(ref a, b);
    }

    public static u128 operator %(u128 a, u128 b)
    {
        u128 c;
        Remainder(out c, ref a, ref b);
        return c;
    }

    public static bool operator <(u128 a, u128 b)
    {
        return LessThan(ref a, ref b);
    }

    public static bool operator <(u128 a, int b)
    {
        return LessThan(ref a, b);
    }

    public static bool operator <(int a, u128 b)
    {
        return LessThan(a, ref b);
    }

    public static bool operator <(u128 a, uint b)
    {
        return LessThan(ref a, b);
    }

    public static bool operator <(uint a, u128 b)
    {
        return LessThan(a, ref b);
    }

    public static bool operator <(u128 a, long b)
    {
        return LessThan(ref a, b);
    }

    public static bool operator <(long a, u128 b)
    {
        return LessThan(a, ref b);
    }

    public static bool operator <(u128 a, ulong b)
    {
        return LessThan(ref a, b);
    }

    public static bool operator <(ulong a, u128 b)
    {
        return LessThan(a, ref b);
    }

    public static bool operator <=(u128 a, u128 b)
    {
        return !LessThan(ref b, ref a);
    }

    public static bool operator <=(u128 a, int b)
    {
        return !LessThan(b, ref a);
    }

    public static bool operator <=(int a, u128 b)
    {
        return !LessThan(ref b, a);
    }

    public static bool operator <=(u128 a, uint b)
    {
        return !LessThan(b, ref a);
    }

    public static bool operator <=(uint a, u128 b)
    {
        return !LessThan(ref b, a);
    }

    public static bool operator <=(u128 a, long b)
    {
        return !LessThan(b, ref a);
    }

    public static bool operator <=(long a, u128 b)
    {
        return !LessThan(ref b, a);
    }

    public static bool operator <=(u128 a, ulong b)
    {
        return !LessThan(b, ref a);
    }

    public static bool operator <=(ulong a, u128 b)
    {
        return !LessThan(ref b, a);
    }

    public static bool operator >(u128 a, u128 b)
    {
        return LessThan(ref b, ref a);
    }

    public static bool operator >(u128 a, int b)
    {
        return LessThan(b, ref a);
    }

    public static bool operator >(int a, u128 b)
    {
        return LessThan(ref b, a);
    }

    public static bool operator >(u128 a, uint b)
    {
        return LessThan(b, ref a);
    }

    public static bool operator >(uint a, u128 b)
    {
        return LessThan(ref b, a);
    }

    public static bool operator >(u128 a, long b)
    {
        return LessThan(b, ref a);
    }

    public static bool operator >(long a, u128 b)
    {
        return LessThan(ref b, a);
    }

    public static bool operator >(u128 a, ulong b)
    {
        return LessThan(b, ref a);
    }

    public static bool operator >(ulong a, u128 b)
    {
        return LessThan(ref b, a);
    }

    public static bool operator >=(u128 a, u128 b)
    {
        return !LessThan(ref a, ref b);
    }

    public static bool operator >=(u128 a, int b)
    {
        return !LessThan(ref a, b);
    }

    public static bool operator >=(int a, u128 b)
    {
        return !LessThan(a, ref b);
    }

    public static bool operator >=(u128 a, uint b)
    {
        return !LessThan(ref a, b);
    }

    public static bool operator >=(uint a, u128 b)
    {
        return !LessThan(a, ref b);
    }

    public static bool operator >=(u128 a, long b)
    {
        return !LessThan(ref a, b);
    }

    public static bool operator >=(long a, u128 b)
    {
        return !LessThan(a, ref b);
    }

    public static bool operator >=(u128 a, ulong b)
    {
        return !LessThan(ref a, b);
    }

    public static bool operator >=(ulong a, u128 b)
    {
        return !LessThan(a, ref b);
    }

    public static bool operator ==(u128 a, u128 b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(u128 a, int b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(int a, u128 b)
    {
        return b.Equals(a);
    }

    public static bool operator ==(u128 a, uint b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(uint a, u128 b)
    {
        return b.Equals(a);
    }

    public static bool operator ==(u128 a, long b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(long a, u128 b)
    {
        return b.Equals(a);
    }

    public static bool operator ==(u128 a, ulong b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(ulong a, u128 b)
    {
        return b.Equals(a);
    }

    public static bool operator !=(u128 a, u128 b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(u128 a, int b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(int a, u128 b)
    {
        return !b.Equals(a);
    }

    public static bool operator !=(u128 a, uint b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(uint a, u128 b)
    {
        return !b.Equals(a);
    }

    public static bool operator !=(u128 a, long b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(long a, u128 b)
    {
        return !b.Equals(a);
    }

    public static bool operator !=(u128 a, ulong b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(ulong a, u128 b)
    {
        return !b.Equals(a);
    }

    public int CompareTo(u128 other)
    {
        if (s1 != other.s1)
            return s1.CompareTo(other.s1);
        return s0.CompareTo(other.s0);
    }

    public int CompareTo(int other)
    {
        if (s1 != 0 || other < 0)
            return 1;
        return s0.CompareTo((ulong)other);
    }

    public int CompareTo(uint other)
    {
        if (s1 != 0)
            return 1;
        return s0.CompareTo((ulong)other);
    }

    public int CompareTo(long other)
    {
        if (s1 != 0 || other < 0)
            return 1;
        return s0.CompareTo((ulong)other);
    }

    public int CompareTo(ulong other)
    {
        if (s1 != 0)
            return 1;
        return s0.CompareTo(other);
    }

    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;
        if (!(obj is u128))
            throw new ArgumentException();
        return CompareTo((u128)obj);
    }

    private static bool LessThan(ref u128 a, long b)
    {
        return b >= 0 && a.s1 == 0 && a.s0 < (ulong)b;
    }

    private static bool LessThan(long a, ref u128 b)
    {
        return a < 0 || b.s1 != 0 || (ulong)a < b.s0;
    }

    private static bool LessThan(ref u128 a, ulong b)
    {
        return a.s1 == 0 && a.s0 < b;
    }

    private static bool LessThan(ulong a, ref u128 b)
    {
        return b.s1 != 0 || a < b.s0;
    }

    private static bool LessThan(ref u128 a, ref u128 b)
    {
        if (a.s1 != b.s1)
            return a.s1 < b.s1;
        return a.s0 < b.s0;
    }

    public static bool Equals(ref u128 a, ref u128 b)
    {
        return a.s0 == b.s0 && a.s1 == b.s1;
    }

    public bool Equals(u128 other)
    {
        return s0 == other.s0 && s1 == other.s1;
    }

    public bool Equals(int other)
    {
        return other >= 0 && s0 == (uint)other && s1 == 0;
    }

    public bool Equals(uint other)
    {
        return s0 == other && s1 == 0;
    }

    public bool Equals(long other)
    {
        return other >= 0 && s0 == (ulong)other && s1 == 0;
    }

    public bool Equals(ulong other)
    {
        return s0 == other && s1 == 0;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is u128))
            return false;
        return Equals((u128)obj);
    }

    public override int GetHashCode()
    {
        return s0.GetHashCode() ^ s1.GetHashCode();
    }

    public static void Multiply(out u128 c, ulong a, ulong b)
    {
        c = burst.umul128(a, b);
    }

    public static void Multiply(out u128 c, ref u128 a, uint b)
    {
        if (a.s1 == 0)
            Multiply64(out c, a.s0, b);
        else
            Multiply128(out c, ref a, b);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == (BigInteger)a * (BigInteger)b % ((BigInteger)1 << 128));
    }

    public static void Multiply(out u128 c, ref u128 a, ulong b)
    {
        if (a.s1 == 0)
            Multiply64(out c, a.s0, b);
        else
            Multiply128(out c, ref a, b);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == (BigInteger)a * (BigInteger)b % ((BigInteger)1 << 128));
    }

    public static void Multiply(out u128 c, ref u128 a, ref u128 b)
    {
        if ((a.s1 | b.s1) == 0)
            Multiply64(out c, a.s0, b.s0);
        else if (a.s1 == 0)
            Multiply128(out c, ref b, a.s0);
        else if (b.s1 == 0)
            Multiply128(out c, ref a, b.s0);
        else
            Multiply128(out c, ref a, ref b);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == (BigInteger)a * (BigInteger)b % ((BigInteger)1 << 128));
    }

    private static void Multiply(out u256 c, ref u128 a, ref u128 b)
    {
#if true
        u128 c00,
            c01,
            c10,
            c11;
        Multiply64(out c00, a.s0, b.s0);
        Multiply64(out c01, a.s0, b.s1);
        Multiply64(out c10, a.s1, b.s0);
        Multiply64(out c11, a.s1, b.s1);
        var carry1 = (uint)0;
        var carry2 = (uint)0;
        c.s0 = c00.S0;
        c.s1 = Add(Add(c00.s1, c01.s0, ref carry1), c10.s0, ref carry1);
        c.s2 = Add(Add(Add(c01.s1, c10.s1, ref carry2), c11.s0, ref carry2), carry1, ref carry2);
        c.s3 = c11.s1 + carry2;
#else
        // Karatsuba method.
        // Warning: doesn't correctly handle overflow.
        u128 z0, z1, z2;
        Multiply64(out z0, a.s0, b.s0);
        Multiply64(out z2, a.s1, b.s1);
        Multiply64(out z1, a.s0 + a.s1, b.s0 + b.s1);
        Subtract(ref z1, ref z2);
        Subtract(ref z1, ref z0);
        var carry1 = (uint)0;
        var carry2 = (uint)0;
        c.s0 = z0.S0;
        c.s1 = Add(z0.s1, z1.s0, ref carry1);
        c.s2 = Add(Add(z1.s1, z2.s0, ref carry2), carry1, ref carry2);
        c.s3 = z2.s1 + carry2;
#endif
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == (BigInteger)a * (BigInteger)b);
    }

    public static u128 Abs(u128 a)
    {
        return a;
    }

    public static u128 Square(ulong a)
    {
        u128 c;
        Square(out c, a);
        return c;
    }

    public static u128 Square(u128 a)
    {
        u128 c;
        Square(out c, ref a);
        return c;
    }

    public static void Square(out u128 c, ulong a)
    {
        Square64(out c, a);
    }

    public static void Square(out u128 c, ref u128 a)
    {
        if (a.s1 == 0)
            Square64(out c, a.s0);
        else
            Multiply128(out c, ref a, ref a);
    }

    public static u128 Cube(ulong a)
    {
        u128 c;
        Cube(out c, a);
        return c;
    }

    public static u128 Cube(u128 a)
    {
        u128 c;
        Cube(out c, ref a);
        return c;
    }

    public static void Cube(out u128 c, ulong a)
    {
        u128 square;
        Square(out square, a);
        Multiply(out c, ref square, a);
    }

    public static void Cube(out u128 c, ref u128 a)
    {
        u128 square;
        if (a.s1 == 0)
        {
            Square64(out square, a.s0);
            Multiply(out c, ref square, a.s0);
        }
        else
        {
            Multiply128(out square, ref a, ref a);
            Multiply128(out c, ref square, ref a);
        }
    }

    public static void Add(out u128 c, ulong a, ulong b)
    {
        c.s0 = a + b;
        c.s1 = 0;
        if (c.s0 < a && c.s0 < b)
            ++c.s1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == ((BigInteger)a + (BigInteger)b));
    }

    public static void Add(out u128 c, ref u128 a, ulong b)
    {
        c.s0 = a.s0 + b;
        c.s1 = a.s1;
        if (c.s0 < a.s0 && c.s0 < b)
            ++c.s1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == ((BigInteger)a + (BigInteger)b) % ((BigInteger)1 << 128));
    }

    public static void Add(out u128 c, ref u128 a, ref u128 b)
    {
        c.s0 = a.s0 + b.s0;
        c.s1 = a.s1 + b.s1;
        if (c.s0 < a.s0 && c.s0 < b.s0)
            ++c.s1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == ((BigInteger)a + (BigInteger)b) % ((BigInteger)1 << 128));
    }

    private static ulong Add(ulong a, ulong b, ref uint carry)
    {
        var c = a + b;
        if (c < a && c < b)
            ++carry;
        return c;
    }

    public static void Add(ref u128 a, ulong b)
    {
        var sum = a.s0 + b;
        if (sum < a.s0 && sum < b)
            ++a.s1;
        a.s0 = sum;
    }

    public static void Add(ref u128 a, ref u128 b)
    {
        var sum = a.s0 + b.s0;
        if (sum < a.s0 && sum < b.s0)
            ++a.s1;
        a.s0 = sum;
        a.s1 += b.s1;
    }

    public static void Add(ref u128 a, u128 b)
    {
        Add(ref a, ref b);
    }

    public static void Subtract(out u128 c, ref u128 a, ulong b)
    {
        c.s0 = a.s0 - b;
        c.s1 = a.s1;
        if (a.s0 < b)
            --c.s1;
        UnityEngine.Assertions.Assert.IsTrue(
            (BigInteger)c
                == ((BigInteger)a - (BigInteger)b + ((BigInteger)1 << 128)) % ((BigInteger)1 << 128)
        );
    }

    public static void Subtract(out u128 c, ulong a, ref u128 b)
    {
        c.s0 = a - b.s0;
        c.s1 = 0 - b.s1;
        if (a < b.s0)
            --c.s1;
        UnityEngine.Assertions.Assert.IsTrue(
            (BigInteger)c
                == ((BigInteger)a - (BigInteger)b + ((BigInteger)1 << 128)) % ((BigInteger)1 << 128)
        );
    }

    public static void Subtract(out u128 c, ref u128 a, ref u128 b)
    {
        c.s0 = a.s0 - b.s0;
        c.s1 = a.s1 - b.s1;
        if (a.s0 < b.s0)
            --c.s1;
        UnityEngine.Assertions.Assert.IsTrue(
            (BigInteger)c
                == ((BigInteger)a - (BigInteger)b + ((BigInteger)1 << 128)) % ((BigInteger)1 << 128)
        );
    }

    public static void Subtract(ref u128 a, ulong b)
    {
        if (a.s0 < b)
            --a.s1;
        a.s0 -= b;
    }

    public static void Subtract(ref u128 a, ref u128 b)
    {
        if (a.s0 < b.s0)
            --a.s1;
        a.s0 -= b.s0;
        a.s1 -= b.s1;
    }

    public static void Subtract(ref u128 a, u128 b)
    {
        Subtract(ref a, ref b);
    }

    private static void Square64(out u128 w, ulong u)
    {
        var u0 = (ulong)(uint)u;
        var u1 = u >> 32;
        var carry = u0 * u0;
        var r0 = (uint)carry;
        var u0u1 = u0 * u1;
        carry = (carry >> 32) + u0u1;
        var r2 = carry >> 32;
        carry = (uint)carry + u0u1;
        w.s0 = carry << 32 | r0;
        w.s1 = (carry >> 32) + r2 + u1 * u1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * u);
    }

    private static void Multiply64(out u128 w, uint u, uint v)
    {
        w.s0 = (ulong)u * v;
        w.s1 = 0;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * v);
    }

    private static void Multiply64(out u128 w, ulong u, uint v)
    {
        var u0 = (ulong)(uint)u;
        var u1 = u >> 32;
        var carry = u0 * v;
        var r0 = (uint)carry;
        carry = (carry >> 32) + u1 * v;
        w.s0 = carry << 32 | r0;
        w.s1 = carry >> 32;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * v);
    }

    private static void Multiply64(out u128 w, ulong u, ulong v)
    {
        var u0 = (ulong)(uint)u;
        var u1 = u >> 32;
        var v0 = (ulong)(uint)v;
        var v1 = v >> 32;
        var carry = u0 * v0;
        var r0 = (uint)carry;
        carry = (carry >> 32) + u0 * v1;
        var r2 = carry >> 32;
        carry = (uint)carry + u1 * v0;
        w.s0 = carry << 32 | r0;
        w.s1 = (carry >> 32) + r2 + u1 * v1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * v);
    }

    private static void Multiply64(out u128 w, ulong u, ulong v, ulong c)
    {
        var u0 = (ulong)(uint)u;
        var u1 = u >> 32;
        var v0 = (ulong)(uint)v;
        var v1 = v >> 32;
        var carry = u0 * v0 + (uint)c;
        var r0 = (uint)carry;
        carry = (carry >> 32) + u0 * v1 + (c >> 32);
        var r2 = carry >> 32;
        carry = (uint)carry + u1 * v0;
        w.s0 = carry << 32 | r0;
        w.s1 = (carry >> 32) + r2 + u1 * v1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * v + c);
    }

    private static ulong MultiplyHigh64(ulong u, ulong v, ulong c)
    {
        var u0 = (ulong)(uint)u;
        var u1 = u >> 32;
        var v0 = (ulong)(uint)v;
        var v1 = v >> 32;
        var carry = ((u0 * v0 + (uint)c) >> 32) + u0 * v1 + (c >> 32);
        var r2 = carry >> 32;
        carry = (uint)carry + u1 * v0;
        return (carry >> 32) + r2 + u1 * v1;
    }

    private static void Multiply128(out u128 w, ref u128 u, uint v)
    {
        Multiply64(out w, u.s0, v);
        w.s1 += u.s1 * v;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * v % ((BigInteger)1 << 128));
    }

    private static void Multiply128(out u128 w, ref u128 u, ulong v)
    {
        Multiply64(out w, u.s0, v);
        w.s1 += u.s1 * v;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * v % ((BigInteger)1 << 128));
    }

    private static void Multiply128(out u128 w, ref u128 u, ref u128 v)
    {
        Multiply64(out w, u.s0, v.s0);
        w.s1 += u.s1 * v.s0 + u.s0 * v.s1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u * v % ((BigInteger)1 << 128));
    }

    public static void Divide(out u128 w, ref u128 u, uint v)
    {
        if (u.s1 == 0)
            Divide64(out w, u.s0, v);
        else if (u.s1 <= uint.MaxValue)
            Divide96(out w, ref u, v);
        else
            Divide128(out w, ref u, v);
    }

    public static void Divide(out u128 w, ref u128 u, ulong v)
    {
        if (u.s1 == 0)
            Divide64(out w, u.s0, v);
        else
        {
            var v0 = (uint)v;
            if (v == v0)
            {
                if (u.s1 <= uint.MaxValue)
                    Divide96(out w, ref u, v0);
                else
                    Divide128(out w, ref u, v0);
            }
            else
            {
                if (u.s1 <= uint.MaxValue)
                    Divide96(out w, ref u, v);
                else
                    Divide128(out w, ref u, v);
            }
        }
    }

    public static void Divide(out u128 c, ref u128 a, ref u128 b)
    {
        if (LessThan(ref a, ref b))
            c = Zero;
        else if (b.s1 == 0)
            Divide(out c, ref a, b.s0);
        else if (b.s1 <= uint.MaxValue)
        {
            u128 rem;
            Create(out c, DivRem96(out rem, ref a, ref b));
        }
        else
        {
            u128 rem;
            Create(out c, DivRem128(out rem, ref a, ref b));
        }
    }

    public static uint Remainder(ref u128 u, uint v)
    {
        if (u.s1 == 0)
            return (uint)(u.s0 % v);
        if (u.s1 <= uint.MaxValue)
            return Remainder96(ref u, v);
        return Remainder128(ref u, v);
    }

    public static ulong Remainder(ref u128 u, ulong v)
    {
        if (u.s1 == 0)
            return u.s0 % v;
        var v0 = (uint)v;
        if (v == v0)
        {
            if (u.s1 <= uint.MaxValue)
                return Remainder96(ref u, v0);
            return Remainder128(ref u, v0);
        }
        if (u.s1 <= uint.MaxValue)
            return Remainder96(ref u, v);
        return Remainder128(ref u, v);
    }

    public static void Remainder(out u128 c, ref u128 a, ref u128 b)
    {
        if (LessThan(ref a, ref b))
            c = a;
        else if (b.s1 == 0)
            Create(out c, Remainder(ref a, b.s0));
        else if (b.s1 <= uint.MaxValue)
            DivRem96(out c, ref a, ref b);
        else
            DivRem128(out c, ref a, ref b);
    }

    public static void Remainder(ref u128 a, ref u128 b)
    {
        u128 a2 = a;
        Remainder(out a, ref a2, ref b);
    }

    private static void Remainder(out u128 c, ref u256 a, ref u128 b)
    {
        if (b.r3 == 0)
            Remainder192(out c, ref a, ref b);
        else
            Remainder256(out c, ref a, ref b);
    }

    private static void Divide64(out u128 w, ulong u, ulong v)
    {
        w.s1 = 0;
        w.s0 = u / v;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u / v);
    }

    private static void Divide96(out u128 w, ref u128 u, uint v)
    {
        var r2 = u.r2;
        var w2 = r2 / v;
        var u0 = (ulong)(r2 - w2 * v);
        var u0u1 = u0 << 32 | u.r1;
        var w1 = (uint)(u0u1 / v);
        u0 = u0u1 - w1 * v;
        u0u1 = u0 << 32 | u.r0;
        var w0 = (uint)(u0u1 / v);
        w.s1 = w2;
        w.s0 = (ulong)w1 << 32 | w0;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u / v);
    }

    private static void Divide128(out u128 w, ref u128 u, uint v)
    {
        var r3 = u.r3;
        var w3 = r3 / v;
        var u0 = (ulong)(r3 - w3 * v);
        var u0u1 = u0 << 32 | u.r2;
        var w2 = (uint)(u0u1 / v);
        u0 = u0u1 - w2 * v;
        u0u1 = u0 << 32 | u.r1;
        var w1 = (uint)(u0u1 / v);
        u0 = u0u1 - w1 * v;
        u0u1 = u0 << 32 | u.r0;
        var w0 = (uint)(u0u1 / v);
        w.s1 = (ulong)w3 << 32 | w2;
        w.s0 = (ulong)w1 << 32 | w0;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u / v);
    }

    private static void Divide96(out u128 w, ref u128 u, ulong v)
    {
        w.s0 = w.s1 = 0;
        var dneg = GetBitLength((uint)(v >> 32));
        var d = 32 - dneg;
        var vPrime = v << d;
        var v1 = (uint)(vPrime >> 32);
        var v2 = (uint)vPrime;
        var r0 = u.r0;
        var r1 = u.r1;
        var r2 = u.r2;
        var r3 = (uint)0;
        if (d != 0)
        {
            r3 = r2 >> dneg;
            r2 = r2 << d | r1 >> dneg;
            r1 = r1 << d | r0 >> dneg;
            r0 <<= d;
        }
        var q1 = DivRem(r3, ref r2, ref r1, v1, v2);
        var q0 = DivRem(r2, ref r1, ref r0, v1, v2);
        w.s0 = (ulong)q1 << 32 | q0;
        w.s1 = 0;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u / v);
    }

    private static void Divide128(out u128 w, ref u128 u, ulong v)
    {
        w.s0 = w.s1 = 0;
        var dneg = GetBitLength((uint)(v >> 32));
        var d = 32 - dneg;
        var vPrime = v << d;
        var v1 = (uint)(vPrime >> 32);
        var v2 = (uint)vPrime;
        var r0 = u.r0;
        var r1 = u.r1;
        var r2 = u.r2;
        var r3 = u.r3;
        var r4 = (uint)0;
        if (d != 0)
        {
            r4 = r3 >> dneg;
            r3 = r3 << d | r2 >> dneg;
            r2 = r2 << d | r1 >> dneg;
            r1 = r1 << d | r0 >> dneg;
            r0 <<= d;
        }
        w.s1 = DivRem(r4, ref r3, ref r2, v1, v2);
        var q1 = DivRem(r3, ref r2, ref r1, v1, v2);
        var q0 = DivRem(r2, ref r1, ref r0, v1, v2);
        w.s0 = (ulong)q1 << 32 | q0;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)w == (BigInteger)u / v);
    }

    private static uint Remainder96(ref u128 u, uint v)
    {
        var u0 = (ulong)(u.r2 % v);
        var u0u1 = u0 << 32 | u.r1;
        u0 = u0u1 % v;
        u0u1 = u0 << 32 | u.r0;
        return (uint)(u0u1 % v);
    }

    private static uint Remainder128(ref u128 u, uint v)
    {
        var u0 = (ulong)(u.r3 % v);
        var u0u1 = u0 << 32 | u.r2;
        u0 = u0u1 % v;
        u0u1 = u0 << 32 | u.r1;
        u0 = u0u1 % v;
        u0u1 = u0 << 32 | u.r0;
        return (uint)(u0u1 % v);
    }

    private static ulong Remainder96(ref u128 u, ulong v)
    {
        var dneg = GetBitLength((uint)(v >> 32));
        var d = 32 - dneg;
        var vPrime = v << d;
        var v1 = (uint)(vPrime >> 32);
        var v2 = (uint)vPrime;
        var r0 = u.r0;
        var r1 = u.r1;
        var r2 = u.r2;
        var r3 = (uint)0;
        if (d != 0)
        {
            r3 = r2 >> dneg;
            r2 = r2 << d | r1 >> dneg;
            r1 = r1 << d | r0 >> dneg;
            r0 <<= d;
        }
        DivRem(r3, ref r2, ref r1, v1, v2);
        DivRem(r2, ref r1, ref r0, v1, v2);
        return ((ulong)r1 << 32 | r0) >> d;
    }

    private static ulong Remainder128(ref u128 u, ulong v)
    {
        var dneg = GetBitLength((uint)(v >> 32));
        var d = 32 - dneg;
        var vPrime = v << d;
        var v1 = (uint)(vPrime >> 32);
        var v2 = (uint)vPrime;
        var r0 = u.r0;
        var r1 = u.r1;
        var r2 = u.r2;
        var r3 = u.r3;
        var r4 = (uint)0;
        if (d != 0)
        {
            r4 = r3 >> dneg;
            r3 = r3 << d | r2 >> dneg;
            r2 = r2 << d | r1 >> dneg;
            r1 = r1 << d | r0 >> dneg;
            r0 <<= d;
        }
        DivRem(r4, ref r3, ref r2, v1, v2);
        DivRem(r3, ref r2, ref r1, v1, v2);
        DivRem(r2, ref r1, ref r0, v1, v2);
        return ((ulong)r1 << 32 | r0) >> d;
    }

    private static ulong DivRem96(out u128 rem, ref u128 a, ref u128 b)
    {
        var d = 32 - GetBitLength(b.r2);
        u128 v;
        LeftShift64(out v, ref b, d);
        var r4 = (uint)LeftShift64(out rem, ref a, d);
        var v1 = v.r2;
        var v2 = v.r1;
        var v3 = v.r0;
        var r3 = rem.r3;
        var r2 = rem.r2;
        var r1 = rem.r1;
        var r0 = rem.r0;
        var q1 = DivRem(r4, ref r3, ref r2, ref r1, v1, v2, v3);
        var q0 = DivRem(r3, ref r2, ref r1, ref r0, v1, v2, v3);
        Create(out rem, r0, r1, r2, 0);
        var div = (ulong)q1 << 32 | q0;
        RightShift64(ref rem, d);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)div == (BigInteger)a / (BigInteger)b);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)rem == (BigInteger)a % (BigInteger)b);
        return div;
    }

    private static uint DivRem128(out u128 rem, ref u128 a, ref u128 b)
    {
        var d = 32 - GetBitLength(b.r3);
        u128 v;
        LeftShift64(out v, ref b, d);
        var r4 = (uint)LeftShift64(out rem, ref a, d);
        var r3 = rem.r3;
        var r2 = rem.r2;
        var r1 = rem.r1;
        var r0 = rem.r0;
        var div = DivRem(r4, ref r3, ref r2, ref r1, ref r0, v.r3, v.r2, v.r1, v.r0);
        Create(out rem, r0, r1, r2, r3);
        RightShift64(ref rem, d);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)div == (BigInteger)a / (BigInteger)b);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)rem == (BigInteger)a % (BigInteger)b);
        return div;
    }

    private static void Remainder192(out u128 c, ref u256 a, ref u128 b)
    {
        var d = 32 - GetBitLength(b.r2);
        u128 v;
        LeftShift64(out v, ref b, d);
        var v1 = v.r2;
        var v2 = v.r1;
        var v3 = v.r0;
        u256 rem;
        LeftShift64(out rem, ref a, d);
        var r6 = rem.r6;
        var r5 = rem.r5;
        var r4 = rem.r4;
        var r3 = rem.r3;
        var r2 = rem.r2;
        var r1 = rem.r1;
        var r0 = rem.r0;
        DivRem(r6, ref r5, ref r4, ref r3, v1, v2, v3);
        DivRem(r5, ref r4, ref r3, ref r2, v1, v2, v3);
        DivRem(r4, ref r3, ref r2, ref r1, v1, v2, v3);
        DivRem(r3, ref r2, ref r1, ref r0, v1, v2, v3);
        Create(out c, r0, r1, r2, 0);
        RightShift64(ref c, d);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == (BigInteger)a % (BigInteger)b);
    }

    private static void Remainder256(out u128 c, ref u256 a, ref u128 b)
    {
        var d = 32 - GetBitLength(b.r3);
        u128 v;
        LeftShift64(out v, ref b, d);
        var v1 = v.r3;
        var v2 = v.r2;
        var v3 = v.r1;
        var v4 = v.r0;
        u256 rem;
        var r8 = (uint)LeftShift64(out rem, ref a, d);
        var r7 = rem.r7;
        var r6 = rem.r6;
        var r5 = rem.r5;
        var r4 = rem.r4;
        var r3 = rem.r3;
        var r2 = rem.r2;
        var r1 = rem.r1;
        var r0 = rem.r0;
        DivRem(r8, ref r7, ref r6, ref r5, ref r4, v1, v2, v3, v4);
        DivRem(r7, ref r6, ref r5, ref r4, ref r3, v1, v2, v3, v4);
        DivRem(r6, ref r5, ref r4, ref r3, ref r2, v1, v2, v3, v4);
        DivRem(r5, ref r4, ref r3, ref r2, ref r1, v1, v2, v3, v4);
        DivRem(r4, ref r3, ref r2, ref r1, ref r0, v1, v2, v3, v4);
        Create(out c, r0, r1, r2, r3);
        RightShift64(ref c, d);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == (BigInteger)a % (BigInteger)b);
    }

    private static ulong Q(uint u0, uint u1, uint u2, uint v1, uint v2)
    {
        var u0u1 = (ulong)u0 << 32 | u1;
        var qhat = u0 == v1 ? uint.MaxValue : u0u1 / v1;
        var r = u0u1 - qhat * v1;
        if (r == (uint)r && v2 * qhat > (r << 32 | u2))
        {
            --qhat;
            r += v1;
            if (r == (uint)r && v2 * qhat > (r << 32 | u2))
            {
                --qhat;
                r += v1;
            }
        }
        return qhat;
    }

    private static uint DivRem(uint u0, ref uint u1, ref uint u2, uint v1, uint v2)
    {
        var qhat = Q(u0, u1, u2, v1, v2);
        var carry = qhat * v2;
        var borrow = (long)u2 - (uint)carry;
        carry >>= 32;
        u2 = (uint)borrow;
        borrow >>= 32;
        carry += qhat * v1;
        borrow += (long)u1 - (uint)carry;
        carry >>= 32;
        u1 = (uint)borrow;
        borrow >>= 32;
        borrow += (long)u0 - (uint)carry;
        if (borrow != 0)
        {
            --qhat;
            carry = (ulong)u2 + v2;
            u2 = (uint)carry;
            carry >>= 32;
            carry += (ulong)u1 + v1;
            u1 = (uint)carry;
        }
        return (uint)qhat;
    }

    private static uint DivRem(
        uint u0,
        ref uint u1,
        ref uint u2,
        ref uint u3,
        uint v1,
        uint v2,
        uint v3
    )
    {
        var qhat = Q(u0, u1, u2, v1, v2);
        var carry = qhat * v3;
        var borrow = (long)u3 - (uint)carry;
        carry >>= 32;
        u3 = (uint)borrow;
        borrow >>= 32;
        carry += qhat * v2;
        borrow += (long)u2 - (uint)carry;
        carry >>= 32;
        u2 = (uint)borrow;
        borrow >>= 32;
        carry += qhat * v1;
        borrow += (long)u1 - (uint)carry;
        carry >>= 32;
        u1 = (uint)borrow;
        borrow >>= 32;
        borrow += (long)u0 - (uint)carry;
        if (borrow != 0)
        {
            --qhat;
            carry = (ulong)u3 + v3;
            u3 = (uint)carry;
            carry >>= 32;
            carry += (ulong)u2 + v2;
            u2 = (uint)carry;
            carry >>= 32;
            carry += (ulong)u1 + v1;
            u1 = (uint)carry;
        }
        return (uint)qhat;
    }

    private static uint DivRem(
        uint u0,
        ref uint u1,
        ref uint u2,
        ref uint u3,
        ref uint u4,
        uint v1,
        uint v2,
        uint v3,
        uint v4
    )
    {
        var qhat = Q(u0, u1, u2, v1, v2);
        var carry = qhat * v4;
        var borrow = (long)u4 - (uint)carry;
        carry >>= 32;
        u4 = (uint)borrow;
        borrow >>= 32;
        carry += qhat * v3;
        borrow += (long)u3 - (uint)carry;
        carry >>= 32;
        u3 = (uint)borrow;
        borrow >>= 32;
        carry += qhat * v2;
        borrow += (long)u2 - (uint)carry;
        carry >>= 32;
        u2 = (uint)borrow;
        borrow >>= 32;
        carry += qhat * v1;
        borrow += (long)u1 - (uint)carry;
        carry >>= 32;
        u1 = (uint)borrow;
        borrow >>= 32;
        borrow += (long)u0 - (uint)carry;
        if (borrow != 0)
        {
            --qhat;
            carry = (ulong)u4 + v4;
            u4 = (uint)carry;
            carry >>= 32;
            carry += (ulong)u3 + v3;
            u3 = (uint)carry;
            carry >>= 32;
            carry += (ulong)u2 + v2;
            u2 = (uint)carry;
            carry >>= 32;
            carry += (ulong)u1 + v1;
            u1 = (uint)carry;
        }
        return (uint)qhat;
    }

    public static void ModAdd(out u128 c, ref u128 a, ref u128 b, ref u128 modulus)
    {
        Add(out c, ref a, ref b);
        if (!LessThan(ref c, ref modulus) || LessThan(ref c, ref a) && LessThan(ref c, ref b))
            Subtract(ref c, ref modulus);
    }

    public static void ModSub(out u128 c, ref u128 a, ref u128 b, ref u128 modulus)
    {
        Subtract(out c, ref a, ref b);
        if (LessThan(ref a, ref b))
            Add(ref c, ref modulus);
    }

    public static void ModMul(out u128 c, ref u128 a, ref u128 b, ref u128 modulus)
    {
        if (modulus.s1 == 0)
        {
            u128 product;
            Multiply64(out product, a.s0, b.s0);
            Create(out c, Remainder(ref product, modulus.s0));
        }
        else
        {
            u256 product;
            Multiply(out product, ref a, ref b);
            Remainder(out c, ref product, ref modulus);
        }
    }

    public static void ModMul(ref u128 a, ref u128 b, ref u128 modulus)
    {
        if (modulus.s1 == 0)
        {
            u128 product;
            Multiply64(out product, a.s0, b.s0);
            Create(out a, Remainder(ref product, modulus.s0));
        }
        else
        {
            u256 product;
            Multiply(out product, ref a, ref b);
            Remainder(out a, ref product, ref modulus);
        }
    }

    public static void ModPow(out u128 result, ref u128 value, ref u128 exponent, ref u128 modulus)
    {
        result = one;
        var v = value;
        var e = exponent.s0;
        if (exponent.s1 != 0)
        {
            for (var i = 0; i < 64; i++)
            {
                if ((e & 1) != 0)
                    ModMul(ref result, ref v, ref modulus);
                ModMul(ref v, ref v, ref modulus);
                e >>= 1;
            }
            e = exponent.s1;
        }
        while (e != 0)
        {
            if ((e & 1) != 0)
                ModMul(ref result, ref v, ref modulus);
            if (e != 1)
                ModMul(ref v, ref v, ref modulus);
            e >>= 1;
        }
        UnityEngine.Assertions.Assert.IsTrue(BigInteger.ModPow(value, exponent, modulus) == result);
    }

    public static void Shift(out u128 c, ref u128 a, int d)
    {
        if (d < 0)
            RightShift(out c, ref a, -d);
        else
            LeftShift(out c, ref a, d);
    }

    public static void ArithmeticShift(out u128 c, ref u128 a, int d)
    {
        if (d < 0)
            ArithmeticRightShift(out c, ref a, -d);
        else
            LeftShift(out c, ref a, d);
    }

    public static ulong LeftShift64(out u128 c, ref u128 a, int d)
    {
        if (d == 0)
        {
            c = a;
            return 0;
        }
        var dneg = 64 - d;
        c.s1 = a.s1 << d | a.s0 >> dneg;
        c.s0 = a.s0 << d;
        return a.s1 >> dneg;
    }

    private static ulong LeftShift64(out u256 c, ref u256 a, int d)
    {
        if (d == 0)
        {
            c = a;
            return 0;
        }
        var dneg = 64 - d;
        c.s3 = a.s3 << d | a.s2 >> dneg;
        c.s2 = a.s2 << d | a.s1 >> dneg;
        c.s1 = a.s1 << d | a.s0 >> dneg;
        c.s0 = a.s0 << d;
        return a.s3 >> dneg;
    }

    public static void LeftShift(out u128 c, ref u128 a, int b)
    {
        if (b < 64)
            LeftShift64(out c, ref a, b);
        else if (b == 64)
        {
            c.s0 = 0;
            c.s1 = a.s0;
            return;
        }
        else
        {
            c.s0 = 0;
            c.s1 = a.s0 << (b - 64);
        }
    }

    public static void RightShift64(out u128 c, ref u128 a, int b)
    {
        if (b == 0)
            c = a;
        else
        {
            c.s0 = a.s0 >> b | a.s1 << (64 - b);
            c.s1 = a.s1 >> b;
        }
    }

    public static void RightShift(out u128 c, ref u128 a, int b)
    {
        if (b < 64)
            RightShift64(out c, ref a, b);
        else if (b == 64)
        {
            c.s0 = a.s1;
            c.s1 = 0;
        }
        else
        {
            c.s0 = a.s1 >> (b - 64);
            c.s1 = 0;
        }
    }

    public static void ArithmeticRightShift64(out u128 c, ref u128 a, int b)
    {
        if (b == 0)
            c = a;
        else
        {
            c.s0 = a.s0 >> b | a.s1 << (64 - b);
            c.s1 = (ulong)((long)a.s1 >> b);
        }
    }

    public static void ArithmeticRightShift(out u128 c, ref u128 a, int b)
    {
        if (b < 64)
            ArithmeticRightShift64(out c, ref a, b);
        else if (b == 64)
        {
            c.s0 = a.s1;
            c.s1 = (ulong)((long)a.s1 >> 63);
        }
        else
        {
            c.s0 = a.s1 >> (b - 64);
            c.s1 = (ulong)((long)a.s1 >> 63);
        }
    }

    public static void And(out u128 c, ref u128 a, ref u128 b)
    {
        c.s0 = a.s0 & b.s0;
        c.s1 = a.s1 & b.s1;
    }

    public static void Or(out u128 c, ref u128 a, ref u128 b)
    {
        c.s0 = a.s0 | b.s0;
        c.s1 = a.s1 | b.s1;
    }

    public static void ExclusiveOr(out u128 c, ref u128 a, ref u128 b)
    {
        c.s0 = a.s0 ^ b.s0;
        c.s1 = a.s1 ^ b.s1;
    }

    public static void Not(out u128 c, ref u128 a)
    {
        c.s0 = ~a.s0;
        c.s1 = ~a.s1;
    }

    public static void Negate(ref u128 a)
    {
        var s0 = a.s0;
        a.s0 = 0 - s0;
        a.s1 = 0 - a.s1;
        if (s0 > 0)
            --a.s1;
    }

    public static void Negate(out u128 c, ref u128 a)
    {
        c.s0 = 0 - a.s0;
        c.s1 = 0 - a.s1;
        if (a.s0 > 0)
            --c.s1;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)c == (BigInteger)(~a + 1));
    }

    public static void Pow(out u128 result, ref u128 value, uint exponent)
    {
        result = one;
        while (exponent != 0)
        {
            if ((exponent & 1) != 0)
            {
                var previous = result;
                Multiply(out result, ref previous, ref value);
            }
            if (exponent != 1)
            {
                var previous = value;
                Square(out value, ref previous);
            }
            exponent >>= 1;
        }
    }

    public static u128 Pow(u128 value, uint exponent)
    {
        u128 result;
        Pow(out result, ref value, exponent);
        return result;
    }

    private const int maxRepShift = 53;
    private static readonly ulong maxRep = (ulong)1 << maxRepShift;
    private static readonly u128 maxRepSquaredHigh = (ulong)1 << (2 * maxRepShift - 64);

    public static ulong FloorSqrt(u128 a)
    {
        if (a.s1 == 0 && a.s0 <= maxRep)
            return (ulong)sqrt(a.s0);
        var s = (ulong)sqrt(ConvertToDouble(ref a));
        if (a.s1 < maxRepSquaredHigh)
        {
            u128 s2;
            Square(out s2, s);
            var r = a.s0 - s2.s0;
            if (r > long.MaxValue)
                --s;
            else if (r - (s << 1) <= long.MaxValue)
                ++s;
            UnityEngine.Assertions.Assert.IsTrue((BigInteger)s * s <= a && (BigInteger)(s + 1) * (s + 1) > a);
            return s;
        }
        s = FloorSqrt(ref a, s);
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)s * s <= a && (BigInteger)(s + 1) * (s + 1) > a);
        return s;
    }

    public static ulong CeilingSqrt(u128 a)
    {
        if (a.s1 == 0 && a.s0 <= maxRep)
            return (ulong)ceil(sqrt(a.s0));
        var s = (ulong)ceil(sqrt(ConvertToDouble(ref a)));
        if (a.s1 < maxRepSquaredHigh)
        {
            u128 s2;
            Square(out s2, s);
            var r = s2.s0 - a.s0;
            if (r > long.MaxValue)
                ++s;
            else if (r - (s << 1) <= long.MaxValue)
                --s;
            UnityEngine.Assertions.Assert.IsTrue((BigInteger)(s - 1) * (s - 1) < a && (BigInteger)s * s >= a);
            return s;
        }
        s = FloorSqrt(ref a, s);
        u128 square;
        Square(out square, s);
        if (square.S0 != a.S0 || square.S1 != a.S1)
            ++s;
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)(s - 1) * (s - 1) < a && (BigInteger)s * s >= a);
        return s;
    }

    private static ulong FloorSqrt(ref u128 a, ulong s)
    {
        var sprev = (ulong)0;
        u128 div;
        u128 sum;
        while (true)
        {
            // Equivalent to:
            // snext = (a / s + s) / 2;
            Divide(out div, ref a, s);
            Add(out sum, ref div, s);
            var snext = sum.S0 >> 1;
            if (sum.S1 != 0)
                snext |= (ulong)1 << 63;
            if (snext == sprev)
            {
                if (snext < s)
                    s = snext;
                break;
            }
            sprev = s;
            s = snext;
        }
        return s;
    }

    public static ulong FloorCbrt(u128 a)
    {
        var s = (ulong)pow(ConvertToDouble(ref a), (double)1 / 3);
        u128 s3;
        Cube(out s3, s);
        if (a < s3)
            --s;
        else
        {
            u128 sum;
            Multiply(out sum, 3 * s, s + 1);
            u128 diff;
            Subtract(out diff, ref a, ref s3);
            if (LessThan(ref sum, ref diff))
                ++s;
        }
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)s * s * s <= a && (BigInteger)(s + 1) * (s + 1) * (s + 1) > a);
        return s;
    }

    public static ulong CeilingCbrt(u128 a)
    {
        var s = (ulong)ceil(pow(ConvertToDouble(ref a), (double)1 / 3));
        u128 s3;
        Cube(out s3, s);
        if (s3 < a)
            ++s;
        else
        {
            u128 sum;
            Multiply(out sum, 3 * s, s + 1);
            u128 diff;
            Subtract(out diff, ref s3, ref a);
            if (LessThan(ref sum, ref diff))
                --s;
        }
        UnityEngine.Assertions.Assert.IsTrue((BigInteger)(s - 1) * (s - 1) * (s - 1) < a && (BigInteger)s * s * s >= a);
        return s;
    }

    public static u128 Min(u128 a, u128 b)
    {
        if (LessThan(ref a, ref b))
            return a;
        return b;
    }

    public static u128 Max(u128 a, u128 b)
    {
        if (LessThan(ref b, ref a))
            return a;
        return b;
    }

    public static double Log(u128 a)
    {
        return Log(a, Math.E);
    }

    public static double Log10(u128 a)
    {
        return Log(a, 10);
    }

    public static double Log(u128 a, double b)
    {
        return Math.Log(ConvertToDouble(ref a), b);
    }

    public static u128 Add(u128 a, u128 b)
    {
        u128 c;
        Add(out c, ref a, ref b);
        return c;
    }

    public static u128 Subtract(u128 a, u128 b)
    {
        u128 c;
        Subtract(out c, ref a, ref b);
        return c;
    }

    public static u128 Multiply(u128 a, u128 b)
    {
        u128 c;
        Multiply(out c, ref a, ref b);
        return c;
    }

    public static u128 Divide(u128 a, u128 b)
    {
        u128 c;
        Divide(out c, ref a, ref b);
        return c;
    }

    public static u128 Remainder(u128 a, u128 b)
    {
        u128 c;
        Remainder(out c, ref a, ref b);
        return c;
    }

    public static u128 DivRem(u128 a, u128 b, out u128 remainder)
    {
        u128 c;
        Divide(out c, ref a, ref b);
        Remainder(out remainder, ref a, ref b);
        return c;
    }

    public static u128 ModAdd(u128 a, u128 b, u128 modulus)
    {
        u128 c;
        ModAdd(out c, ref a, ref b, ref modulus);
        return c;
    }

    public static u128 ModSub(u128 a, u128 b, u128 modulus)
    {
        u128 c;
        ModSub(out c, ref a, ref b, ref modulus);
        return c;
    }

    public static u128 ModMul(u128 a, u128 b, u128 modulus)
    {
        u128 c;
        ModMul(out c, ref a, ref b, ref modulus);
        return c;
    }

    public static u128 ModPow(u128 value, u128 exponent, u128 modulus)
    {
        u128 result;
        ModPow(out result, ref value, ref exponent, ref modulus);
        return result;
    }

    public static u128 Negate(u128 a)
    {
        u128 c;
        Negate(out c, ref a);
        return c;
    }

    public static u128 GreatestCommonDivisor(u128 a, u128 b)
    {
        u128 c;
        GreatestCommonDivisor(out c, ref a, ref b);
        return c;
    }

    private static void RightShift64(ref u128 c, int d)
    {
        if (d == 0)
            return;
        c.s0 = c.s1 << (64 - d) | c.s0 >> d;
        c.s1 >>= d;
    }

    public static void RightShift(ref u128 c, int d)
    {
        if (d < 64)
            RightShift64(ref c, d);
        else
        {
            c.s0 = c.s1 >> (d - 64);
            c.s1 = 0;
        }
    }

    public static void Shift(ref u128 c, int d)
    {
        if (d < 0)
            RightShift(ref c, -d);
        else
            LeftShift(ref c, d);
    }

    public static void ArithmeticShift(ref u128 c, int d)
    {
        if (d < 0)
            ArithmeticRightShift(ref c, -d);
        else
            LeftShift(ref c, d);
    }

    public static void RightShift(ref u128 c)
    {
        c.s0 = c.s1 << 63 | c.s0 >> 1;
        c.s1 >>= 1;
    }

    private static void ArithmeticRightShift64(ref u128 c, int d)
    {
        if (d == 0)
            return;
        c.s0 = c.s1 << (64 - d) | c.s0 >> d;
        c.s1 = (ulong)((long)c.s1 >> d);
    }

    public static void ArithmeticRightShift(ref u128 c, int d)
    {
        if (d < 64)
            ArithmeticRightShift64(ref c, d);
        else
        {
            c.s0 = (ulong)((long)c.s1 >> (d - 64));
            c.s1 = 0;
        }
    }

    public static void ArithmeticRightShift(ref u128 c)
    {
        c.s0 = c.s1 << 63 | c.s0 >> 1;
        c.s1 = (ulong)((long)c.s1 >> 1);
    }

    private static ulong LeftShift64(ref u128 c, int d)
    {
        if (d == 0)
            return 0;
        var dneg = 64 - d;
        var result = c.s1 >> dneg;
        c.s1 = c.s1 << d | c.s0 >> dneg;
        c.s0 <<= d;
        return result;
    }

    public static void LeftShift(ref u128 c, int d)
    {
        if (d < 64)
            LeftShift64(ref c, d);
        else
        {
            c.s1 = c.s0 << (d - 64);
            c.s0 = 0;
        }
    }

    public static void LeftShift(ref u128 c)
    {
        c.s1 = c.s1 << 1 | c.s0 >> 63;
        c.s0 <<= 1;
    }

    public static void Swap(ref u128 a, ref u128 b)
    {
        var as0 = a.s0;
        var as1 = a.s1;
        a.s0 = b.s0;
        a.s1 = b.s1;
        b.s0 = as0;
        b.s1 = as1;
    }

    public static void GreatestCommonDivisor(out u128 c, ref u128 a, ref u128 b)
    {
        // Check whether one number is > 64 bits and the other is <= 64 bits and both are non-zero.
        u128 a1,
            b1;
        if ((a.s1 == 0) != (b.s1 == 0) && !a.IsZero && !b.IsZero)
        {
            // Perform a normal step so that both a and b are <= 64 bits.
            if (LessThan(ref a, ref b))
            {
                a1 = a;
                Remainder(out b1, ref b, ref a);
            }
            else
            {
                b1 = b;
                Remainder(out a1, ref a, ref b);
            }
        }
        else
        {
            a1 = a;
            b1 = b;
        }

        // Make sure neither is zero.
        if (a1.IsZero)
        {
            c = b1;
            return;
        }
        if (b1.IsZero)
        {
            c = a1;
            return;
        }

        // Ensure a >= b.
        if (LessThan(ref a1, ref b1))
            Swap(ref a1, ref b1);

        // Lehmer-Euclid algorithm.
        // See: http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.31.693
        while (a1.s1 != 0 && !b.IsZero)
        {
            // Extract the high 63 bits of a and b.
            var norm = 63 - GetBitLength(a1.s1);
            u128 ahat,
                bhat;
            Shift(out ahat, ref a1, norm);
            Shift(out bhat, ref b1, norm);
            var uhat = (long)ahat.s1;
            var vhat = (long)bhat.s1;

            // Check whether q exceeds single-precision.
            if (vhat == 0)
            {
                // Perform a normal step and try again.
                u128 rem;
                Remainder(out rem, ref a1, ref b1);
                a1 = b1;
                b1 = rem;
                continue;
            }

            // Perform steps using signed single-precision arithmetic.
            var x0 = (long)1;
            var y0 = (long)0;
            var x1 = (long)0;
            var y1 = (long)1;
            var even = true;
            while (true)
            {
                // Calculate quotient, cosquence pair, and update uhat and vhat.
                var q = uhat / vhat;
                var x2 = x0 - q * x1;
                var y2 = y0 - q * y1;
                var t = uhat;
                uhat = vhat;
                vhat = t - q * vhat;
                even = !even;

                // Apply Jebelean's termination condition
                // to check whether q is valid.
                if (even)
                {
                    if (vhat < -x2 || uhat - vhat < y2 - y1)
                        break;
                }
                else
                {
                    if (vhat < -y2 || uhat - vhat < x2 - x1)
                        break;
                }

                // Adjust cosequence history.
                x0 = x1;
                y0 = y1;
                x1 = x2;
                y1 = y2;
            }

            // Check whether a normal step is necessary.
            if (x0 == 1 && y0 == 0)
            {
                u128 rem;
                Remainder(out rem, ref a1, ref b1);
                a1 = b1;
                b1 = rem;
                continue;
            }

            // Back calculate a and b from the last valid cosequence pairs.
            u128 anew,
                bnew;
            if (even)
            {
                AddProducts(out anew, y0, ref b1, x0, ref a1);
                AddProducts(out bnew, x1, ref a1, y1, ref b1);
            }
            else
            {
                AddProducts(out anew, x0, ref a1, y0, ref b1);
                AddProducts(out bnew, y1, ref b1, x1, ref a1);
            }
            a1 = anew;
            b1 = bnew;
        }

        // Check whether we have any 64 bit work left.
        if (!b1.IsZero)
        {
            var a2 = a1.s0;
            var b2 = b1.s0;

            // Perform 64 bit steps.
            while (a2 > uint.MaxValue && b2 != 0)
            {
                var t = a2 % b2;
                a2 = b2;
                b2 = t;
            }

            // Check whether we have any 32 bit work left.
            if (b2 != 0)
            {
                var a3 = (uint)a2;
                var b3 = (uint)b2;

                // Perform 32 bit steps.
                while (b3 != 0)
                {
                    var t = a3 % b3;
                    a3 = b3;
                    b3 = t;
                }

                Create(out c, a3);
            }
            else
                Create(out c, a2);
        }
        else
            c = a1;
    }

    private static void AddProducts(out u128 result, long x, ref u128 u, long y, ref u128 v)
    {
        // Compute x * u + y * v assuming y is negative and the result is positive and fits in 128 bits.
        u128 product1;
        Multiply(out product1, ref u, (ulong)x);
        u128 product2;
        Multiply(out product2, ref v, (ulong)(-y));
        Subtract(out result, ref product1, ref product2);
    }

    public static int Compare(u128 a, u128 b)
    {
        return a.CompareTo(b);
    }

    private static byte[] bitLength = Enumerable
        .Range(0, byte.MaxValue + 1)
        .Select(value =>
        {
            int count;
            for (count = 0; value != 0; count++)
                value >>= 1;
            return (byte)count;
        })
        .ToArray();

    private static int GetBitLength(uint value)
    {
        var tt = value >> 16;
        if (tt != 0)
        {
            var t = tt >> 8;
            if (t != 0)
                return bitLength[t] + 24;
            return bitLength[tt] + 16;
        }
        else
        {
            var t = value >> 8;
            if (t != 0)
                return bitLength[t] + 8;
            return bitLength[value];
        }
    }

    private static int GetBitLength(ulong value)
    {
        var r1 = value >> 32;
        if (r1 != 0)
            return GetBitLength((uint)r1) + 32;
        return GetBitLength((uint)value);
    }

    public static void Reduce(out u128 w, ref u128 u, ref u128 v, ref u128 n, ulong k0)
    {
        u128 carry;
        Multiply64(out carry, u.s0, v.s0);
        var t0 = carry.s0;
        Multiply64(out carry, u.s1, v.s0, carry.s1);
        var t1 = carry.s0;
        var t2 = carry.s1;

        var m = t0 * k0;
        Multiply64(out carry, m, n.s1, MultiplyHigh64(m, n.s0, t0));
        Add(ref carry, t1);
        t0 = carry.s0;
        Add(out carry, carry.s1, t2);
        t1 = carry.s0;
        t2 = carry.s1;

        Multiply64(out carry, u.s0, v.s1, t0);
        t0 = carry.s0;
        Multiply64(out carry, u.s1, v.s1, carry.s1);
        Add(ref carry, t1);
        t1 = carry.s0;
        Add(out carry, carry.s1, t2);
        t2 = carry.s0;
        var t3 = carry.s1;

        m = t0 * k0;
        Multiply64(out carry, m, n.s1, MultiplyHigh64(m, n.s0, t0));
        Add(ref carry, t1);
        t0 = carry.s0;
        Add(out carry, carry.s1, t2);
        t1 = carry.s0;
        t2 = t3 + carry.s1;

        Create(out w, t0, t1);
        if (t2 != 0 || !LessThan(ref w, ref n))
            Subtract(ref w, ref n);
    }

    public static void Reduce(out u128 w, ref u128 t, ref u128 n, ulong k0)
    {
        u128 carry;
        var t0 = t.s0;
        var t1 = t.s1;
        var t2 = (ulong)0;

        for (var i = 0; i < 2; i++)
        {
            var m = t0 * k0;
            Multiply64(out carry, m, n.s1, MultiplyHigh64(m, n.s0, t0));
            Add(ref carry, t1);
            t0 = carry.s0;
            Add(out carry, carry.s1, t2);
            t1 = carry.s0;
            t2 = carry.s1;
        }

        Create(out w, t0, t1);
        if (t2 != 0 || !LessThan(ref w, ref n))
            Subtract(ref w, ref n);
    }

    public static u128 Reduce(u128 u, u128 v, u128 n, ulong k0)
    {
        u128 w;
        Reduce(out w, ref u, ref v, ref n, k0);
        return w;
    }

    public static u128 Reduce(u128 t, u128 n, ulong k0)
    {
        u128 w;
        Reduce(out w, ref t, ref n, k0);
        return w;
    }
}
