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
using System.Numerics;
using UnityEngine.Assertions;

namespace System;

public struct i128 : IFormattable, IComparable, IComparable<i128>, IEquatable<i128>
{
#pragma warning disable IDE1006 // Naming Styles
    private u128 v;
#pragma warning restore IDE1006 // Naming Styles

    public static i128 MinValue { get; } = (i128)((u128)1 << 127);

    public static i128 MaxValue { get; } = (i128)(((u128)1 << 127) - 1);

    public static i128 Zero { get; } = 0;

    public static i128 One { get; } = 1;

    public static i128 MinusOne { get; } = -1;

    public static i128 Parse(string value)
    {
        i128 c;
        if (!TryParse(value, out c))
            throw new FormatException();
        return c;
    }

    public static bool TryParse(string value, out i128 result)
    {
        return TryParse(value, NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out result);
    }

    public static bool TryParse(
        string value,
        NumberStyles style,
        IFormatProvider format,
        out i128 result
    )
    {
        BigInteger a;
        if (!BigInteger.TryParse(value, style, format, out a))
        {
            result = Zero;
            return false;
        }

        u128.Create(out result.v, a);
        return true;
    }

    public i128(long value)
    {
        u128.Create(out v, value);
    }

    public i128(ulong value)
    {
        u128.Create(out v, value);
    }

    public i128(double value)
    {
        u128.Create(out v, value);
    }

    public i128(decimal value)
    {
        u128.Create(out v, value);
    }

    public i128(BigInteger value)
    {
        u128.Create(out v, value);
    }

    public ulong S0 => v.S0;
    public ulong S1 => v.S1;

    public bool IsZero => v.IsZero;
    public bool IsOne => v.IsOne;

    public bool IsPowerOfTwo => v.IsPowerOfTwo;
    public bool IsEven => v.IsEven;
    public bool IsNegative => v.S1 > long.MaxValue;

    public int Sign => IsNegative ? -1 : v.Sign;

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

    public static explicit operator i128(double a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static implicit operator i128(sbyte a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static implicit operator i128(byte a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static implicit operator i128(short a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static implicit operator i128(ushort a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static implicit operator i128(int a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static implicit operator i128(uint a)
    {
        i128 c;
        u128.Create(out c.v, (ulong)a);
        return c;
    }

    public static implicit operator i128(long a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static implicit operator i128(ulong a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static explicit operator i128(decimal a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static explicit operator i128(u128 a)
    {
        i128 c;
        c.v = a;
        return c;
    }

    public static explicit operator u128(i128 a)
    {
        return a.v;
    }

    public static explicit operator i128(BigInteger a)
    {
        i128 c;
        u128.Create(out c.v, a);
        return c;
    }

    public static explicit operator sbyte(i128 a)
    {
        return (sbyte)a.v.S0;
    }

    public static explicit operator byte(i128 a)
    {
        return (byte)a.v.S0;
    }

    public static explicit operator short(i128 a)
    {
        return (short)a.v.S0;
    }

    public static explicit operator ushort(i128 a)
    {
        return (ushort)a.v.S0;
    }

    public static explicit operator int(i128 a)
    {
        return (int)a.v.S0;
    }

    public static explicit operator uint(i128 a)
    {
        return (uint)a.v.S0;
    }

    public static explicit operator long(i128 a)
    {
        return (long)a.v.S0;
    }

    public static explicit operator ulong(i128 a)
    {
        return a.v.S0;
    }

    public static explicit operator decimal(i128 a)
    {
        if (a.IsNegative)
        {
            u128 c;
            u128.Negate(out c, ref a.v);
            return -(decimal)c;
        }

        return (decimal)a.v;
    }

    public static implicit operator BigInteger(i128 a)
    {
        if (a.IsNegative)
        {
            u128 c;
            u128.Negate(out c, ref a.v);
            return -(BigInteger)c;
        }

        return a.v;
    }

    public static explicit operator float(i128 a)
    {
        if (a.IsNegative)
        {
            u128 c;
            u128.Negate(out c, ref a.v);
            return -u128.ConvertToFloat(ref c);
        }

        return u128.ConvertToFloat(ref a.v);
    }

    public static explicit operator double(i128 a)
    {
        if (a.IsNegative)
        {
            u128 c;
            u128.Negate(out c, ref a.v);
            return -u128.ConvertToDouble(ref c);
        }

        return u128.ConvertToDouble(ref a.v);
    }

    public static i128 operator <<(i128 a, int b)
    {
        i128 c;
        u128.LeftShift(out c.v, ref a.v, b);
        return c;
    }

    public static i128 operator >> (i128 a, int b)
    {
        i128 c;
        u128.ArithmeticRightShift(out c.v, ref a.v, b);
        return c;
    }

    public static i128 operator &(i128 a, i128 b)
    {
        i128 c;
        u128.And(out c.v, ref a.v, ref b.v);
        return c;
    }

    public static int operator &(i128 a, int b)
    {
        return (int)(a.v & (uint)b);
    }

    public static int operator &(int a, i128 b)
    {
        return (int)(b.v & (uint)a);
    }

    public static long operator &(i128 a, long b)
    {
        return (long)(a.v & (ulong)b);
    }

    public static long operator &(long a, i128 b)
    {
        return (long)(b.v & (ulong)a);
    }

    public static i128 operator |(i128 a, i128 b)
    {
        i128 c;
        u128.Or(out c.v, ref a.v, ref b.v);
        return c;
    }

    public static i128 operator ^(i128 a, i128 b)
    {
        i128 c;
        u128.ExclusiveOr(out c.v, ref a.v, ref b.v);
        return c;
    }

    public static i128 operator ~(i128 a)
    {
        i128 c;
        u128.Not(out c.v, ref a.v);
        return c;
    }

    public static i128 operator +(i128 a, long b)
    {
        i128 c;
        if (b < 0)
            u128.Subtract(out c.v, ref a.v, (ulong)-b);
        else
            u128.Add(out c.v, ref a.v, (ulong)b);
        return c;
    }

    public static i128 operator +(long a, i128 b)
    {
        i128 c;
        if (a < 0)
            u128.Subtract(out c.v, ref b.v, (ulong)-a);
        else
            u128.Add(out c.v, ref b.v, (ulong)a);
        return c;
    }

    public static i128 operator +(i128 a, i128 b)
    {
        i128 c;
        u128.Add(out c.v, ref a.v, ref b.v);
        return c;
    }

    public static i128 operator ++(i128 a)
    {
        i128 c;
        u128.Add(out c.v, ref a.v, 1);
        return c;
    }

    public static i128 operator -(i128 a, long b)
    {
        i128 c;
        if (b < 0)
            u128.Add(out c.v, ref a.v, (ulong)-b);
        else
            u128.Subtract(out c.v, ref a.v, (ulong)b);
        return c;
    }

    public static i128 operator -(i128 a, i128 b)
    {
        i128 c;
        u128.Subtract(out c.v, ref a.v, ref b.v);
        return c;
    }

    public static i128 operator +(i128 a)
    {
        return a;
    }

    public static i128 operator -(i128 a)
    {
        i128 c;
        u128.Negate(out c.v, ref a.v);
        return c;
    }

    public static i128 operator --(i128 a)
    {
        i128 c;
        u128.Subtract(out c.v, ref a.v, 1);
        return c;
    }

    public static i128 operator *(i128 a, int b)
    {
        i128 c;
        Multiply(out c, ref a, b);
        return c;
    }

    public static i128 operator *(int a, i128 b)
    {
        i128 c;
        Multiply(out c, ref b, a);
        return c;
    }

    public static i128 operator *(i128 a, uint b)
    {
        i128 c;
        Multiply(out c, ref a, b);
        return c;
    }

    public static i128 operator *(uint a, i128 b)
    {
        i128 c;
        Multiply(out c, ref b, a);
        return c;
    }

    public static i128 operator *(i128 a, long b)
    {
        i128 c;
        Multiply(out c, ref a, b);
        return c;
    }

    public static i128 operator *(long a, i128 b)
    {
        i128 c;
        Multiply(out c, ref b, a);
        return c;
    }

    public static i128 operator *(i128 a, ulong b)
    {
        i128 c;
        Multiply(out c, ref a, b);
        return c;
    }

    public static i128 operator *(ulong a, i128 b)
    {
        i128 c;
        Multiply(out c, ref b, a);
        return c;
    }

    public static i128 operator *(i128 a, i128 b)
    {
        i128 c;
        Multiply(out c, ref a, ref b);
        return c;
    }

    public static i128 operator /(i128 a, int b)
    {
        i128 c;
        Divide(out c, ref a, b);
        return c;
    }

    public static i128 operator /(i128 a, uint b)
    {
        i128 c;
        Divide(out c, ref a, b);
        return c;
    }

    public static i128 operator /(i128 a, long b)
    {
        i128 c;
        Divide(out c, ref a, b);
        return c;
    }

    public static i128 operator /(i128 a, ulong b)
    {
        i128 c;
        Divide(out c, ref a, b);
        return c;
    }

    public static i128 operator /(i128 a, i128 b)
    {
        i128 c;
        Divide(out c, ref a, ref b);
        return c;
    }

    public static int operator %(i128 a, int b)
    {
        return Remainder(ref a, b);
    }

    public static int operator %(i128 a, uint b)
    {
        return Remainder(ref a, b);
    }

    public static long operator %(i128 a, long b)
    {
        return Remainder(ref a, b);
    }

    public static long operator %(i128 a, ulong b)
    {
        return Remainder(ref a, b);
    }

    public static i128 operator %(i128 a, i128 b)
    {
        i128 c;
        Remainder(out c, ref a, ref b);
        return c;
    }

    public static bool operator <(i128 a, u128 b)
    {
        return a.CompareTo(b) < 0;
    }

    public static bool operator <(u128 a, i128 b)
    {
        return b.CompareTo(a) > 0;
    }

    public static bool operator <(i128 a, i128 b)
    {
        return LessThan(ref a.v, ref b.v);
    }

    public static bool operator <(i128 a, int b)
    {
        return LessThan(ref a.v, b);
    }

    public static bool operator <(int a, i128 b)
    {
        return LessThan(a, ref b.v);
    }

    public static bool operator <(i128 a, uint b)
    {
        return LessThan(ref a.v, b);
    }

    public static bool operator <(uint a, i128 b)
    {
        return LessThan(a, ref b.v);
    }

    public static bool operator <(i128 a, long b)
    {
        return LessThan(ref a.v, b);
    }

    public static bool operator <(long a, i128 b)
    {
        return LessThan(a, ref b.v);
    }

    public static bool operator <(i128 a, ulong b)
    {
        return LessThan(ref a.v, b);
    }

    public static bool operator <(ulong a, i128 b)
    {
        return LessThan(a, ref b.v);
    }

    public static bool operator <=(i128 a, u128 b)
    {
        return a.CompareTo(b) <= 0;
    }

    public static bool operator <=(u128 a, i128 b)
    {
        return b.CompareTo(a) >= 0;
    }

    public static bool operator <=(i128 a, i128 b)
    {
        return !LessThan(ref b.v, ref a.v);
    }

    public static bool operator <=(i128 a, int b)
    {
        return !LessThan(b, ref a.v);
    }

    public static bool operator <=(int a, i128 b)
    {
        return !LessThan(ref b.v, a);
    }

    public static bool operator <=(i128 a, uint b)
    {
        return !LessThan(b, ref a.v);
    }

    public static bool operator <=(uint a, i128 b)
    {
        return !LessThan(ref b.v, a);
    }

    public static bool operator <=(i128 a, long b)
    {
        return !LessThan(b, ref a.v);
    }

    public static bool operator <=(long a, i128 b)
    {
        return !LessThan(ref b.v, a);
    }

    public static bool operator <=(i128 a, ulong b)
    {
        return !LessThan(b, ref a.v);
    }

    public static bool operator <=(ulong a, i128 b)
    {
        return !LessThan(ref b.v, a);
    }

    public static bool operator >(i128 a, u128 b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator >(u128 a, i128 b)
    {
        return b.CompareTo(a) < 0;
    }

    public static bool operator >(i128 a, i128 b)
    {
        return LessThan(ref b.v, ref a.v);
    }

    public static bool operator >(i128 a, int b)
    {
        return LessThan(b, ref a.v);
    }

    public static bool operator >(int a, i128 b)
    {
        return LessThan(ref b.v, a);
    }

    public static bool operator >(i128 a, uint b)
    {
        return LessThan(b, ref a.v);
    }

    public static bool operator >(uint a, i128 b)
    {
        return LessThan(ref b.v, a);
    }

    public static bool operator >(i128 a, long b)
    {
        return LessThan(b, ref a.v);
    }

    public static bool operator >(long a, i128 b)
    {
        return LessThan(ref b.v, a);
    }

    public static bool operator >(i128 a, ulong b)
    {
        return LessThan(b, ref a.v);
    }

    public static bool operator >(ulong a, i128 b)
    {
        return LessThan(ref b.v, a);
    }

    public static bool operator >=(i128 a, u128 b)
    {
        return a.CompareTo(b) >= 0;
    }

    public static bool operator >=(u128 a, i128 b)
    {
        return b.CompareTo(a) <= 0;
    }

    public static bool operator >=(i128 a, i128 b)
    {
        return !LessThan(ref a.v, ref b.v);
    }

    public static bool operator >=(i128 a, int b)
    {
        return !LessThan(ref a.v, b);
    }

    public static bool operator >=(int a, i128 b)
    {
        return !LessThan(a, ref b.v);
    }

    public static bool operator >=(i128 a, uint b)
    {
        return !LessThan(ref a.v, b);
    }

    public static bool operator >=(uint a, i128 b)
    {
        return !LessThan(a, ref b.v);
    }

    public static bool operator >=(i128 a, long b)
    {
        return !LessThan(ref a.v, b);
    }

    public static bool operator >=(long a, i128 b)
    {
        return !LessThan(a, ref b.v);
    }

    public static bool operator >=(i128 a, ulong b)
    {
        return !LessThan(ref a.v, b);
    }

    public static bool operator >=(ulong a, i128 b)
    {
        return !LessThan(a, ref b.v);
    }

    public static bool operator ==(u128 a, i128 b)
    {
        return b.Equals(a);
    }

    public static bool operator ==(i128 a, u128 b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(i128 a, i128 b)
    {
        return a.v.Equals(b.v);
    }

    public static bool operator ==(i128 a, int b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(int a, i128 b)
    {
        return b.Equals(a);
    }

    public static bool operator ==(i128 a, uint b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(uint a, i128 b)
    {
        return b.Equals(a);
    }

    public static bool operator ==(i128 a, long b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(long a, i128 b)
    {
        return b.Equals(a);
    }

    public static bool operator ==(i128 a, ulong b)
    {
        return a.Equals(b);
    }

    public static bool operator ==(ulong a, i128 b)
    {
        return b.Equals(a);
    }

    public static bool operator !=(u128 a, i128 b)
    {
        return !b.Equals(a);
    }

    public static bool operator !=(i128 a, u128 b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(i128 a, i128 b)
    {
        return !a.v.Equals(b.v);
    }

    public static bool operator !=(i128 a, int b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(int a, i128 b)
    {
        return !b.Equals(a);
    }

    public static bool operator !=(i128 a, uint b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(uint a, i128 b)
    {
        return !b.Equals(a);
    }

    public static bool operator !=(i128 a, long b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(long a, i128 b)
    {
        return !b.Equals(a);
    }

    public static bool operator !=(i128 a, ulong b)
    {
        return !a.Equals(b);
    }

    public static bool operator !=(ulong a, i128 b)
    {
        return !b.Equals(a);
    }

    public int CompareTo(u128 other)
    {
        if (IsNegative)
            return -1;
        return v.CompareTo(other);
    }

    public int CompareTo(i128 other)
    {
        return SignedCompare(ref v, other.S0, other.S1);
    }

    public int CompareTo(int other)
    {
        return SignedCompare(ref v, (ulong)other, (ulong)(other >> 31));
    }

    public int CompareTo(uint other)
    {
        return SignedCompare(ref v, other, 0);
    }

    public int CompareTo(long other)
    {
        return SignedCompare(ref v, (ulong)other, (ulong)(other >> 63));
    }

    public int CompareTo(ulong other)
    {
        return SignedCompare(ref v, other, 0);
    }

    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;
        if (!(obj is i128))
            throw new ArgumentException();
        return CompareTo((i128)obj);
    }

    private static bool LessThan(ref u128 a, ref u128 b)
    {
        var as1 = (long)a.S1;
        var bs1 = (long)b.S1;
        if (as1 != bs1)
            return as1 < bs1;
        return a.S0 < b.S0;
    }

    private static bool LessThan(ref u128 a, long b)
    {
        var as1 = (long)a.S1;
        var bs1 = b >> 63;
        if (as1 != bs1)
            return as1 < bs1;
        return a.S0 < (ulong)b;
    }

    private static bool LessThan(long a, ref u128 b)
    {
        var as1 = a >> 63;
        var bs1 = (long)b.S1;
        if (as1 != bs1)
            return as1 < bs1;
        return (ulong)a < b.S0;
    }

    private static bool LessThan(ref u128 a, ulong b)
    {
        var as1 = (long)a.S1;
        if (as1 != 0)
            return as1 < 0;
        return a.S0 < b;
    }

    private static bool LessThan(ulong a, ref u128 b)
    {
        var bs1 = (long)b.S1;
        if (0 != bs1)
            return 0 < bs1;
        return a < b.S0;
    }

    private static int SignedCompare(ref u128 a, ulong bs0, ulong bs1)
    {
        var as1 = a.S1;
        if (as1 != bs1)
            return ((long)as1).CompareTo((long)bs1);
        return a.S0.CompareTo(bs0);
    }

    public bool Equals(u128 other)
    {
        return !IsNegative && v.Equals(other);
    }

    public bool Equals(i128 other)
    {
        return v.Equals(other.v);
    }

    public bool Equals(int other)
    {
        if (other < 0)
            return v.S1 == ulong.MaxValue && v.S0 == (uint)other;
        return v.S1 == 0 && v.S0 == (uint)other;
    }

    public bool Equals(uint other)
    {
        return v.S1 == 0 && v.S0 == other;
    }

    public bool Equals(long other)
    {
        if (other < 0)
            return v.S1 == ulong.MaxValue && v.S0 == (ulong)other;
        return v.S1 == 0 && v.S0 == (ulong)other;
    }

    public bool Equals(ulong other)
    {
        return v.S1 == 0 && v.S0 == other;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is i128))
            return false;
        return Equals((i128)obj);
    }

    public override int GetHashCode()
    {
        return v.GetHashCode();
    }

    public static void Multiply(out i128 c, ref i128 a, int b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b < 0)
            {
                u128.Multiply(out c.v, ref aneg, (uint)-b);
            }
            else
            {
                u128.Multiply(out c.v, ref aneg, (uint)b);
                u128.Negate(ref c.v);
            }
        }
        else
        {
            if (b < 0)
            {
                u128.Multiply(out c.v, ref a.v, (uint)-b);
                u128.Negate(ref c.v);
            }
            else
            {
                u128.Multiply(out c.v, ref a.v, (uint)b);
            }
        }

        Assert.IsTrue(c == a * (BigInteger)b);
    }

    public static void Multiply(out i128 c, ref i128 a, uint b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            u128.Multiply(out c.v, ref aneg, b);
            u128.Negate(ref c.v);
        }
        else
        {
            u128.Multiply(out c.v, ref a.v, b);
        }

        Assert.IsTrue(c == a * (BigInteger)b);
    }

    public static void Multiply(out i128 c, ref i128 a, long b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b < 0)
            {
                u128.Multiply(out c.v, ref aneg, (ulong)-b);
            }
            else
            {
                u128.Multiply(out c.v, ref aneg, (ulong)b);
                u128.Negate(ref c.v);
            }
        }
        else
        {
            if (b < 0)
            {
                u128.Multiply(out c.v, ref a.v, (ulong)-b);
                u128.Negate(ref c.v);
            }
            else
            {
                u128.Multiply(out c.v, ref a.v, (ulong)b);
            }
        }

        Assert.IsTrue(c == a * (BigInteger)b);
    }

    public static void Multiply(out i128 c, ref i128 a, ulong b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            u128.Multiply(out c.v, ref aneg, b);
            u128.Negate(ref c.v);
        }
        else
        {
            u128.Multiply(out c.v, ref a.v, b);
        }

        Assert.IsTrue(c == a * (BigInteger)b);
    }

    public static void Multiply(out i128 c, ref i128 a, ref i128 b)
    {
        c = (i128)((u128)a * (u128)b);
        Assert.IsTrue(c == a * (BigInteger)b);
    }

    public static void Divide(out i128 c, ref i128 a, int b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b < 0)
            {
                u128.Multiply(out c.v, ref aneg, (uint)-b);
            }
            else
            {
                u128.Multiply(out c.v, ref aneg, (uint)b);
                u128.Negate(ref c.v);
            }
        }
        else
        {
            if (b < 0)
            {
                u128.Multiply(out c.v, ref a.v, (uint)-b);
                u128.Negate(ref c.v);
            }
            else
            {
                u128.Multiply(out c.v, ref a.v, (uint)b);
            }
        }

        Assert.IsTrue(c == a / (BigInteger)b);
    }

    public static void Divide(out i128 c, ref i128 a, uint b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            u128.Divide(out c.v, ref aneg, b);
            u128.Negate(ref c.v);
        }
        else
        {
            u128.Divide(out c.v, ref a.v, b);
        }

        Assert.IsTrue(c == a / (BigInteger)b);
    }

    public static void Divide(out i128 c, ref i128 a, long b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b < 0)
            {
                u128.Divide(out c.v, ref aneg, (ulong)-b);
            }
            else
            {
                u128.Divide(out c.v, ref aneg, (ulong)b);
                u128.Negate(ref c.v);
            }
        }
        else
        {
            if (b < 0)
            {
                u128.Divide(out c.v, ref a.v, (ulong)-b);
                u128.Negate(ref c.v);
            }
            else
            {
                u128.Divide(out c.v, ref a.v, (ulong)b);
            }
        }

        Assert.IsTrue(c == a / (BigInteger)b);
    }

    public static void Divide(out i128 c, ref i128 a, ulong b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            u128.Divide(out c.v, ref aneg, b);
            u128.Negate(ref c.v);
        }
        else
        {
            u128.Divide(out c.v, ref a.v, b);
        }

        Assert.IsTrue(c == a / (BigInteger)b);
    }

    public static void Divide(out i128 c, ref i128 a, ref i128 b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b.IsNegative)
            {
                u128 bneg;
                u128.Negate(out bneg, ref b.v);
                u128.Divide(out c.v, ref aneg, ref bneg);
            }
            else
            {
                u128.Divide(out c.v, ref aneg, ref b.v);
                u128.Negate(ref c.v);
            }
        }
        else
        {
            if (b.IsNegative)
            {
                u128 bneg;
                u128.Negate(out bneg, ref b.v);
                u128.Divide(out c.v, ref a.v, ref bneg);
                u128.Negate(ref c.v);
            }
            else
            {
                u128.Divide(out c.v, ref a.v, ref b.v);
            }
        }

        Assert.IsTrue(c == a / (BigInteger)b);
    }

    public static int Remainder(ref i128 a, int b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b < 0)
                return (int)u128.Remainder(ref aneg, (uint)-b);
            return -(int)u128.Remainder(ref aneg, (uint)b);
        }

        if (b < 0)
            return -(int)u128.Remainder(ref a.v, (uint)-b);
        return (int)u128.Remainder(ref a.v, (uint)b);
    }

    public static int Remainder(ref i128 a, uint b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            return -(int)u128.Remainder(ref aneg, b);
        }

        return (int)u128.Remainder(ref a.v, b);
    }

    public static long Remainder(ref i128 a, long b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b < 0)
                return (long)u128.Remainder(ref aneg, (ulong)-b);
            return -(long)u128.Remainder(ref aneg, (ulong)b);
        }

        if (b < 0)
            return -(long)u128.Remainder(ref a.v, (ulong)-b);
        return (long)u128.Remainder(ref a.v, (ulong)b);
    }

    public static long Remainder(ref i128 a, ulong b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            return -(long)u128.Remainder(ref aneg, b);
        }

        return (long)u128.Remainder(ref a.v, b);
    }

    public static void Remainder(out i128 c, ref i128 a, ref i128 b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b.IsNegative)
            {
                u128 bneg;
                u128.Negate(out bneg, ref b.v);
                u128.Remainder(out c.v, ref aneg, ref bneg);
            }
            else
            {
                u128.Remainder(out c.v, ref aneg, ref b.v);
                u128.Negate(ref c.v);
            }
        }
        else
        {
            if (b.IsNegative)
            {
                u128 bneg;
                u128.Negate(out bneg, ref b.v);
                u128.Remainder(out c.v, ref a.v, ref bneg);
                u128.Negate(ref c.v);
            }
            else
            {
                u128.Remainder(out c.v, ref a.v, ref b.v);
            }
        }

        Assert.IsTrue(c == a % (BigInteger)b);
    }

    public static i128 Abs(i128 a)
    {
        if (!a.IsNegative)
            return a;
        i128 c;
        u128.Negate(out c.v, ref a.v);
        return c;
    }

    public static i128 Square(long a)
    {
        if (a < 0)
            a = -a;
        i128 c;
        u128.Square(out c.v, (ulong)a);
        return c;
    }

    public static i128 Square(i128 a)
    {
        i128 c;
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            u128.Square(out c.v, ref aneg);
        }
        else
        {
            u128.Square(out c.v, ref a.v);
        }

        return c;
    }

    public static i128 Cube(long a)
    {
        i128 c;
        if (a < 0)
        {
            u128.Cube(out c.v, (ulong)-a);
            u128.Negate(ref c.v);
        }
        else
        {
            u128.Cube(out c.v, (ulong)a);
        }

        return c;
    }

    public static i128 Cube(i128 a)
    {
        i128 c;
        if (a < 0)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            u128.Cube(out c.v, ref aneg);
            u128.Negate(ref c.v);
        }
        else
        {
            u128.Cube(out c.v, ref a.v);
        }

        return c;
    }

    public static void Add(ref i128 a, long b)
    {
        if (b < 0)
            u128.Subtract(ref a.v, (ulong)-b);
        else
            u128.Add(ref a.v, (ulong)b);
    }

    public static void Add(ref i128 a, ref i128 b)
    {
        u128.Add(ref a.v, ref b.v);
    }

    public static void Subtract(ref i128 a, long b)
    {
        if (b < 0)
            u128.Add(ref a.v, (ulong)-b);
        else
            u128.Subtract(ref a.v, (ulong)b);
    }

    public static void Subtract(ref i128 a, ref i128 b)
    {
        u128.Subtract(ref a.v, ref b.v);
    }

    public static void Add(ref i128 a, i128 b)
    {
        u128.Add(ref a.v, ref b.v);
    }

    public static void Subtract(ref i128 a, i128 b)
    {
        u128.Subtract(ref a.v, ref b.v);
    }

    public static void AddProduct(ref i128 a, ref u128 b, int c)
    {
        u128 product;
        if (c < 0)
        {
            u128.Multiply(out product, ref b, (uint)-c);
            u128.Subtract(ref a.v, ref product);
        }
        else
        {
            u128.Multiply(out product, ref b, (uint)c);
            u128.Add(ref a.v, ref product);
        }
    }

    public static void AddProduct(ref i128 a, ref u128 b, long c)
    {
        u128 product;
        if (c < 0)
        {
            u128.Multiply(out product, ref b, (ulong)-c);
            u128.Subtract(ref a.v, ref product);
        }
        else
        {
            u128.Multiply(out product, ref b, (ulong)c);
            u128.Add(ref a.v, ref product);
        }
    }

    public static void SubtractProduct(ref i128 a, ref u128 b, int c)
    {
        u128 d;
        if (c < 0)
        {
            u128.Multiply(out d, ref b, (uint)-c);
            u128.Add(ref a.v, ref d);
        }
        else
        {
            u128.Multiply(out d, ref b, (uint)c);
            u128.Subtract(ref a.v, ref d);
        }
    }

    public static void SubtractProduct(ref i128 a, ref u128 b, long c)
    {
        u128 d;
        if (c < 0)
        {
            u128.Multiply(out d, ref b, (ulong)-c);
            u128.Add(ref a.v, ref d);
        }
        else
        {
            u128.Multiply(out d, ref b, (ulong)c);
            u128.Subtract(ref a.v, ref d);
        }
    }

    public static void AddProduct(ref i128 a, u128 b, int c)
    {
        AddProduct(ref a, ref b, c);
    }

    public static void AddProduct(ref i128 a, u128 b, long c)
    {
        AddProduct(ref a, ref b, c);
    }

    public static void SubtractProduct(ref i128 a, u128 b, int c)
    {
        SubtractProduct(ref a, ref b, c);
    }

    public static void SubtractProduct(ref i128 a, u128 b, long c)
    {
        SubtractProduct(ref a, ref b, c);
    }

    public static void Pow(out i128 result, ref i128 value, int exponent)
    {
        if (exponent < 0)
            throw new ArgumentException("exponent must not be negative");
        if (value.IsNegative)
        {
            u128 valueneg;
            u128.Negate(out valueneg, ref value.v);
            if ((exponent & 1) == 0)
            {
                u128.Pow(out result.v, ref valueneg, (uint)exponent);
            }
            else
            {
                u128.Pow(out result.v, ref valueneg, (uint)exponent);
                u128.Negate(ref result.v);
            }
        }
        else
        {
            u128.Pow(out result.v, ref value.v, (uint)exponent);
        }
    }

    public static i128 Pow(i128 value, int exponent)
    {
        i128 result;
        Pow(out result, ref value, exponent);
        return result;
    }

    public static ulong FloorSqrt(i128 a)
    {
        if (a.IsNegative)
            throw new ArgumentException("argument must not be negative");
        return u128.FloorSqrt(a.v);
    }

    public static ulong CeilingSqrt(i128 a)
    {
        if (a.IsNegative)
            throw new ArgumentException("argument must not be negative");
        return u128.CeilingSqrt(a.v);
    }

    public static long FloorCbrt(i128 a)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            return -(long)u128.FloorCbrt(aneg);
        }

        return (long)u128.FloorCbrt(a.v);
    }

    public static long CeilingCbrt(i128 a)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            return -(long)u128.CeilingCbrt(aneg);
        }

        return (long)u128.CeilingCbrt(a.v);
    }

    public static i128 Min(i128 a, i128 b)
    {
        if (LessThan(ref a.v, ref b.v))
            return a;
        return b;
    }

    public static i128 Max(i128 a, i128 b)
    {
        if (LessThan(ref b.v, ref a.v))
            return a;
        return b;
    }

    public static double Log(i128 a)
    {
        return Log(a, Math.E);
    }

    public static double Log10(i128 a)
    {
        return Log(a, 10);
    }

    public static double Log(i128 a, double b)
    {
        if (a.IsNegative || a.IsZero)
            throw new ArgumentException("argument must be positive");
        return Math.Log(u128.ConvertToDouble(ref a.v), b);
    }

    public static i128 Add(i128 a, i128 b)
    {
        i128 c;
        u128.Add(out c.v, ref a.v, ref b.v);
        return c;
    }

    public static i128 Subtract(i128 a, i128 b)
    {
        i128 c;
        u128.Subtract(out c.v, ref a.v, ref b.v);
        return c;
    }

    public static i128 Multiply(i128 a, i128 b)
    {
        i128 c;
        Multiply(out c, ref a, ref b);
        return c;
    }

    public static i128 Divide(i128 a, i128 b)
    {
        i128 c;
        Divide(out c, ref a, ref b);
        return c;
    }

    public static i128 Remainder(i128 a, i128 b)
    {
        i128 c;
        Remainder(out c, ref a, ref b);
        return c;
    }

    public static i128 DivRem(i128 a, i128 b, out i128 remainder)
    {
        i128 c;
        Divide(out c, ref a, ref b);
        Remainder(out remainder, ref a, ref b);
        return c;
    }

    public static i128 Negate(i128 a)
    {
        i128 c;
        u128.Negate(out c.v, ref a.v);
        return c;
    }

    public static i128 GreatestCommonDivisor(i128 a, i128 b)
    {
        i128 c;
        GreatestCommonDivisor(out c, ref a, ref b);
        return c;
    }

    public static void GreatestCommonDivisor(out i128 c, ref i128 a, ref i128 b)
    {
        if (a.IsNegative)
        {
            u128 aneg;
            u128.Negate(out aneg, ref a.v);
            if (b.IsNegative)
            {
                u128 bneg;
                u128.Negate(out bneg, ref b.v);
                u128.GreatestCommonDivisor(out c.v, ref aneg, ref bneg);
            }
            else
            {
                u128.GreatestCommonDivisor(out c.v, ref aneg, ref b.v);
            }
        }
        else
        {
            if (b.IsNegative)
            {
                u128 bneg;
                u128.Negate(out bneg, ref b.v);
                u128.GreatestCommonDivisor(out c.v, ref a.v, ref bneg);
            }
            else
            {
                u128.GreatestCommonDivisor(out c.v, ref a.v, ref b.v);
            }
        }
    }

    public static void LeftShift(ref i128 c, int d)
    {
        u128.LeftShift(ref c.v, d);
    }

    public static void LeftShift(ref i128 c)
    {
        u128.LeftShift(ref c.v);
    }

    public static void RightShift(ref i128 c, int d)
    {
        u128.ArithmeticRightShift(ref c.v, d);
    }

    public static void RightShift(ref i128 c)
    {
        u128.ArithmeticRightShift(ref c.v);
    }

    public static void Swap(ref i128 a, ref i128 b)
    {
        u128.Swap(ref a.v, ref b.v);
    }

    public static int Compare(i128 a, i128 b)
    {
        return a.CompareTo(b);
    }

    public static void Shift(out i128 c, ref i128 a, int d)
    {
        u128.ArithmeticShift(out c.v, ref a.v, d);
    }

    public static void Shift(ref i128 c, int d)
    {
        u128.ArithmeticShift(ref c.v, d);
    }

    public static i128 ModAdd(i128 a, i128 b, i128 modulus)
    {
        i128 c;
        u128.ModAdd(out c.v, ref a.v, ref b.v, ref modulus.v);
        return c;
    }

    public static i128 ModSub(i128 a, i128 b, i128 modulus)
    {
        i128 c;
        u128.ModSub(out c.v, ref a.v, ref b.v, ref modulus.v);
        return c;
    }

    public static i128 ModMul(i128 a, i128 b, i128 modulus)
    {
        i128 c;
        u128.ModMul(out c.v, ref a.v, ref b.v, ref modulus.v);
        return c;
    }

    public static i128 ModPow(i128 value, i128 exponent, i128 modulus)
    {
        i128 result;
        u128.ModPow(out result.v, ref value.v, ref exponent.v, ref modulus.v);
        return result;
    }
}
