// Based on https://github.com/CodesInChaos/SoftFloat

// Copyright (c) 2011 CodesInChaos

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// The MIT License (MIT) - http://www.opensource.org/licenses/mit-license.php
// If you need a different license please contact me

using System;
using System.Diagnostics;

namespace OpenSage.Mathematics
{
    // Internal representation is identical to IEEE binary32 floatingpoints
    [DebuggerDisplay("{ToStringInv()}")]
    public readonly struct SoftFloat : IEquatable<SoftFloat>, IComparable<SoftFloat>, IComparable, IFormattable
    {
        private readonly uint _raw;

        internal SoftFloat(uint raw)
        {
            _raw = raw;
        }

        private uint RawMantissa { get { return _raw & 0x7FFFFF; } }
        private int Mantissa
        {
            get
            {
                if (RawExponent != 0)
                {
                    var sign = (uint) ((int) _raw >> 31);
                    return (int) (((RawMantissa | 0x800000) ^ sign) - sign);
                }
                else
                {
                    var sign = (uint) ((int) _raw >> 31);
                    return (int) (((RawMantissa) ^ sign) - sign);
                }
            }
        }

        private sbyte Exponent { get { return (sbyte) (RawExponent - ExponentBias); } }

        private byte RawExponent { get { return (byte) (_raw >> MantissaBits); } }


        private const uint SignMask = 0x80000000;
        private const int MantissaBits = 23;
        private const int ExponentBias = 127;

        private const uint RawNaN = 0xFFC00000;//same as float.NaN
        private const uint RawPositiveInfinity = 0x7F800000;
        private const uint RawNegativeInfinity = RawPositiveInfinity ^ SignMask;
        private const uint RawOne = 0x3F800000;
        private const uint RawMinusOne = RawOne ^ SignMask;
        private const uint RawMaxValue = 0x7F7FFFFF;
        private const uint RawMinValue = 0x7F7FFFFF ^ SignMask;
        private const uint RawEpsilon = 0x00000001;
        private const uint RawOneOverLog2Of10 = 0;//Fixme
        private const uint RawOneOverLog2OfE = 0;//Fixme
        private const uint RawLog2OfE = 0;


        public static SoftFloat Zero { get { return new SoftFloat(); } }
        public static SoftFloat PositiveInfinity { get { return new SoftFloat(RawPositiveInfinity); } }
        public static SoftFloat NegativeInfinity { get { return new SoftFloat(RawNegativeInfinity); } }
        public static SoftFloat NaN { get { return new SoftFloat(RawNaN); } }
        public static SoftFloat One { get { return new SoftFloat(RawOne); } }
        public static SoftFloat MinusOne { get { return new SoftFloat(RawMinusOne); } }
        public static SoftFloat MaxValue { get { return new SoftFloat(RawMaxValue); } }
        public static SoftFloat MinValue { get { return new SoftFloat(RawMinValue); } }
        public static SoftFloat Epsilon { get { return new SoftFloat(RawEpsilon); } }

        public override string ToString()
        {
            return ((float) this).ToString();
        }

        public static explicit operator SoftFloat(float f)
        {
            var raw = ReinterpretFloatToInt32(f);
            return new SoftFloat(raw);
        }

        public static explicit operator float(SoftFloat f)
        {
            var raw = f._raw;
            return ReinterpretIntToFloat32(raw);
        }

        public static SoftFloat operator -(SoftFloat f)
        {
            return new SoftFloat(f._raw ^ 0x80000000);
        }

        public static SoftFloat operator +(SoftFloat f1, SoftFloat f2)
        {
            var rawExp1 = f1.RawExponent;
            var rawExp2 = f2.RawExponent;
            var deltaExp = rawExp1 - rawExp2;
            if (deltaExp >= 0)
            {
                if (rawExp1 != 255)
                {//Finite
                    if (deltaExp > 25)
                    {
                        return f1;
                    }

                    int man1;
                    int man2;
                    if (rawExp2 != 0)
                    {
                        //man1 = f1.Mantissa
                        //http://graphics.stanford.edu/~seander/bithacks.html#ConditionalNegate
                        var sign1 = (uint) ((int) f1._raw >> 31);
                        man1 = (int) (((f1.RawMantissa | 0x800000) ^ sign1) - sign1);
                        //man2 = f2.Mantissa
                        var sign2 = (uint) ((int) f2._raw >> 31);
                        man2 = (int) (((f2.RawMantissa | 0x800000) ^ sign2) - sign2);
                    }
                    else
                    {//Subnorm
                     //man2 = f2.Mantissa
                        var sign2 = (uint) ((int) f2._raw >> 31);
                        man2 = (int) ((f2.RawMantissa ^ sign2) - sign2);

                        man1 = f1.Mantissa;

                        rawExp2 = 1;
                        if (rawExp1 == 0)
                        {
                            rawExp1 = 1;
                        }

                        deltaExp = rawExp1 - rawExp2;
                    }
                    var man = (man1 << 6) + ((man2 << 6) >> deltaExp);
                    var absMan = (uint) Math.Abs(man);
                    if (absMan == 0)
                    {
                        return Zero;
                    }

                    var msb = absMan >> 23;
                    var rawExp = rawExp1 - 6;
                    while (msb == 0)
                    {
                        rawExp -= 8;
                        absMan <<= 8;
                        msb = absMan >> 23;
                    }
                    var msbIndex = BitScanReverse8(msb);
                    rawExp += msbIndex;
                    absMan >>= msbIndex;
                    if ((uint) (rawExp - 1) < 254)
                    {
                        var raw = (uint) man & 0x80000000 | (uint) rawExp << 23 | (absMan & 0x7FFFFF);
                        return new SoftFloat(raw);
                    }
                    else
                    {
                        if (rawExp >= 255)
                        {//Overflow
                            if (man >= 0)
                            {
                                return PositiveInfinity;
                            }
                            else
                            {
                                return NegativeInfinity;
                            }
                        }
                        if (rawExp >= -24)//Fixme
                        {
                            var raw = (uint) man & 0x80000000 | absMan >> (-rawExp + 1);
                            return new SoftFloat(raw);
                        }
                        return Zero;
                    }
                }
                else
                {//special

                    if (rawExp2 != 255)//f1 is NaN, +Inf, -Inf and f2 is finite
                    {
                        return f1;
                    }
                    // Both not finite
                    if (f1._raw == f2._raw)
                    {
                        return f1;
                    }
                    else
                    {
                        return NaN;
                    }
                }

            }
            else
            {
                //ToDo manually write this code
                return f2 + f1;//flip operands
            }
        }

        public static SoftFloat operator -(SoftFloat f1, SoftFloat f2)
        {
            return f1 + (-f2);
        }

        public static SoftFloat operator *(SoftFloat f1, SoftFloat f2)
        {
            int man1;
            int rawExp1 = f1.RawExponent;
            uint sign1;
            uint sign2;
            if (rawExp1 == 0)
            {//SubNorm
             //man1 = f1.Mantissa
                sign1 = (uint) ((int) f1._raw >> 31);
                var rawMan1 = f1.RawMantissa;
                if (rawMan1 == 0 && f2.IsFinite())
                {
                    return new SoftFloat((f1._raw ^ f2._raw) & SignMask);
                }

                rawExp1 = 1;
                while ((rawMan1 & 0x800000) == 0)
                {
                    rawMan1 <<= 1;
                    rawExp1--;
                }
                Debug.Assert(rawMan1 >> 23 == 1);
                man1 = (int) ((rawMan1 ^ sign1) - sign1);
            }
            else if (rawExp1 != 255)
            {//Norm
             //man1 = f1.Mantissa
                sign1 = (uint) ((int) f1._raw >> 31);
                man1 = (int) (((f1.RawMantissa | 0x800000) ^ sign1) - sign1);
            }
            else
            {//Non finite
                if (f1._raw == RawPositiveInfinity)
                {
                    if (f2.IsZero())
                    {
                        return NaN;
                    }

                    if (f2.IsNaN())
                    {
                        return f2;
                    }

                    if ((int) f2._raw >= 0)
                    {
                        return PositiveInfinity;
                    }
                    else
                    {
                        return NegativeInfinity;
                    }
                }
                else if (f1._raw == RawNegativeInfinity)
                {
                    if (f2.IsZero())
                    {
                        return NaN;
                    }

                    if (f2.IsNaN())
                    {
                        return f2;
                    }

                    if ((int) f2._raw < 0)
                    {
                        return PositiveInfinity;
                    }
                    else
                    {
                        return NegativeInfinity;
                    }
                }
                else
                {
                    return f1;
                }
            }

            int man2;
            int rawExp2 = f2.RawExponent;
            if (rawExp2 == 0)
            {//SubNorm
             //man2 = f2.Mantissa
                sign2 = (uint) ((int) f2._raw >> 31);
                var rawMan2 = f2.RawMantissa;
                if (rawMan2 == 0)
                {
                    return new SoftFloat((f1._raw ^ f2._raw) & SignMask);
                }

                rawExp2 = 1;
                while ((rawMan2 & 0x800000) == 0)
                {
                    rawMan2 <<= 1;
                    rawExp2--;
                }
                Debug.Assert(rawMan2 >> 23 == 1);
                man2 = (int) ((rawMan2 ^ sign2) - sign2);
            }
            else if (rawExp2 != 255)
            {//Norm
             //man2 = f2.Mantissa
                sign2 = (uint) ((int) f2._raw >> 31);
                man2 = (int) (((f2.RawMantissa | 0x800000) ^ sign2) - sign2);
            }
            else
            {//Non finite
                if (f2._raw == RawPositiveInfinity)
                {
                    if (f1.IsZero())
                    {
                        return NaN;
                    }

                    if ((int) f1._raw >= 0)
                    {
                        return PositiveInfinity;
                    }
                    else
                    {
                        return NegativeInfinity;
                    }
                }
                else if (f2._raw == RawNegativeInfinity)
                {
                    if (f1.IsZero())
                    {
                        return NaN;
                    }

                    if ((int) f1._raw < 0)
                    {
                        return PositiveInfinity;
                    }
                    else
                    {
                        return NegativeInfinity;
                    }
                }
                else
                {
                    return f2;
                }
            }

            var longMan = man1 * (long) man2;
            var man = (int) (longMan >> 23);
            Debug.Assert(man != 0);
            var absMan = (uint) Math.Abs(man);
            var rawExp = rawExp1 + rawExp2 - ExponentBias;
            var sign = (uint) man & 0x80000000;
            if ((absMan & 0x1000000) != 0)
            {
                absMan >>= 1;
                rawExp++;
            }
            Debug.Assert(absMan >> 23 == 1);
            if (rawExp >= 255)
            {
                return new SoftFloat(sign ^ RawPositiveInfinity);//Overflow
            }

            if (rawExp <= 0)
            {//Subnorms/Underflow
                if (rawExp <= -24)//Fixme - check correct value
                {
                    return new SoftFloat(sign);
                }

                absMan >>= -rawExp + 1;
                rawExp = 0;
            }

            var raw = sign | (uint) rawExp << 23 | absMan & 0x7FFFFF;
            return new SoftFloat(raw);
        }

        private static readonly sbyte[] Msb = new sbyte[256];
        private static int BitScanReverse8(uint b)
        {
            return Msb[b];
        }

        private static unsafe uint ReinterpretFloatToInt32(float f)
        {
            return *(uint*) &f;
        }

        private static unsafe float ReinterpretIntToFloat32(uint i)
        {
            return *(float*) &i;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((SoftFloat) obj);
        }

        public bool Equals(SoftFloat other)
        {
            if (RawExponent != 255)
            {
                return (_raw == other._raw) ||
                    ((_raw & 0x7FFFFFFF) == 0) && ((other._raw & 0x7FFFFFFF) == 0);//0==-0
            }
            else
            {
                if (RawMantissa == 0)
                {
                    return _raw == other._raw;//infinities
                }
                else
                {
                    return other.RawMantissa != 0;//NaNs are equal for `Equals` (as opposed to the == operator)
                }
            }
        }

        public override int GetHashCode()
        {
            if (_raw == SignMask)
            {
                return 0;// +0 equals -0
            }

            if (!IsNaN(this))
            {
                return (int) _raw;
            }
            else
            {
                return unchecked((int) RawNaN);//All NaNs are equal
            }
        }

        public static bool operator ==(SoftFloat f1, SoftFloat f2)
        {
            if (f1.RawExponent != 255)
            {
                return (f1._raw == f2._raw) ||
                    ((f1._raw & 0x7FFFFFFF) == 0) && ((f2._raw & 0x7FFFFFFF) == 0);//0==-0
            }
            else
            {
                if (f1.RawMantissa == 0)
                {
                    return f1._raw == f2._raw;//infinities
                }
                else
                {
                    return false;//NaNs
                }
            }
        }

        public static bool operator !=(SoftFloat f1, SoftFloat f2)
        {
            return !(f1 == f2);
        }

        static SoftFloat()
        {
            //Init MostSignificantBit table
            for (var i = 0; i < 256; i++)
            {
                sbyte value = 7;//128-255
                if (i < 128)//64-127
                {
                    value = 6;
                }

                if (i < 64)//32-63
                {
                    value = 5;
                }

                if (i < 32)//16-31
                {
                    value = 4;
                }

                if (i < 16)//8-15
                {
                    value = 3;
                }

                if (i < 8)//4-7
                {
                    value = 2;
                }

                if (i < 4)//2-3
                {
                    value = 1;
                }

                if (i < 2)//1
                {
                    value = 0;
                }

                if (i < 1)//0
                {
                    value = -1;
                }

                Msb[i] = value;
            }
        }

        public static bool operator <(SoftFloat f1, SoftFloat f2)
        {
            if (f1.IsNaN() || f2.IsNaN())
            {
                return false;
            }

            return f1.CompareTo(f2) < 0;
        }

        public static bool operator >(SoftFloat f1, SoftFloat f2)
        {
            if (f1.IsNaN() || f2.IsNaN())
            {
                return false;
            }

            return f1.CompareTo(f2) > 0;
        }

        public static bool operator <=(SoftFloat f1, SoftFloat f2)
        {
            if (f1.IsNaN() || f2.IsNaN())
            {
                return false;
            }

            return f1.CompareTo(f2) <= 0;
        }

        public static bool operator >=(SoftFloat f1, SoftFloat f2)
        {
            if (f1.IsNaN() || f2.IsNaN())
            {
                return false;
            }

            return f1.CompareTo(f2) >= 0;
        }

        public int CompareTo(SoftFloat other)
        {
            if (IsNaN() && other.IsNaN())
            {
                return 0;
            }

            var sign1 = (uint) ((int) _raw >> 31);
            var val1 = (int) (((_raw) ^ (sign1 & 0x7FFFFFFF)) - sign1);

            var sign2 = (uint) ((int) other._raw >> 31);
            var val2 = (int) (((other._raw) ^ (sign2 & 0x7FFFFFFF)) - sign2);
            return val1.CompareTo(val2);
        }

        public static bool IsInfinity(SoftFloat f)
        {
            return (f._raw & 0x7FFFFFFF) == 0x7F800000;
        }

        public bool IsInfinity()
        {
            return (_raw & 0x7FFFFFFF) == 0x7F800000;
        }

        public static bool IsNegativeInfinity(SoftFloat f)
        {
            return f._raw == RawNegativeInfinity;
        }

        public bool IsNegativeInfinity()
        {
            return _raw == RawNegativeInfinity;
        }

        public static bool IsPositiveInfinity(SoftFloat f)
        {
            return f._raw == RawPositiveInfinity;
        }

        public bool IsPositiveInfinity()
        {
            return _raw == RawPositiveInfinity;
        }

        public bool IsNaN()
        {
            return (RawExponent == 255) && !IsInfinity();
        }

        public static bool IsNaN(SoftFloat f)
        {
            return (f.RawExponent == 255) && !IsInfinity(f);
        }

        public static bool IsFinite(SoftFloat f)
        {
            return f.RawExponent != 255;
        }

        public bool IsFinite()
        {
            return RawExponent != 255;
        }

        public bool IsZero()
        {
            return (_raw & 0x7FFFFFFF) == 0;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is SoftFloat))
            {
                throw new ArgumentException("obj");
            }

            return CompareTo((SoftFloat) obj);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ((float) this).ToString(format, formatProvider);
        }

        public string ToString(string format)
        {
            return ((float) this).ToString(format);
        }

        public string ToString(IFormatProvider provider)
        {
            return ((float) this).ToString(provider);
        }

        public string ToStringInv()
        {
            return ((float) this).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static SoftFloat Abs(SoftFloat f)
        {
            if (f.RawExponent != 255 || IsInfinity(f))
            {
                return new SoftFloat(f._raw ^ SignMask);
            }
            else
            {
                return f;//Leave NaN untouched
            }
        }

        //Returns NaN iff either argument is NaN
        public static SoftFloat Max(SoftFloat val1, SoftFloat val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else if (IsNaN(val1))
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        //Returns NaN iff either argument is NaN
        public static SoftFloat Min(SoftFloat val1, SoftFloat val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else if (IsNaN(val1))
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        public static int Sign(SoftFloat value)
        {
            if (IsNaN(value))
            {
                throw new ArithmeticException("Sign doesn't support NaN argument");
            }

            if ((value._raw & 0x7FFFFFFF) == 0)
            {
                return 0;
            }
            else if ((int) value >= 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public uint ToIeeeRaw()
        {
            return _raw;
        }

        public static long RawDistance(SoftFloat f1, SoftFloat f2)
        {
            if (!(IsFinite(f1) && IsFinite(f2)))
            {
                if (f1.Equals(f2))
                {
                    return 0;
                }
                else
                {
                    return long.MaxValue;
                }
            }
            else
            {
                var sign1 = (uint) ((int) f1._raw >> 31);
                var val1 = (int) (((f1._raw) ^ (sign1 & 0x7FFFFFFF)) - sign1);

                var sign2 = (uint) ((int) f2._raw >> 31);
                var val2 = (int) (((f2._raw) ^ (sign2 & 0x7FFFFFFF)) - sign2);

                return Math.Abs(val1 - (long) val2);
            }
        }

        public static SoftFloat FromIeeeRaw(uint ieeeRaw)
        {
            return new SoftFloat(ieeeRaw);
        }
    }
}
