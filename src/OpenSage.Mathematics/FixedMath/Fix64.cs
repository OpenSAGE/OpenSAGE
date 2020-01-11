using System;
using System.IO;
using System.Runtime.CompilerServices;

// Based on https://github.com/asik/FixedMath.Net
// Licensed under Apache license, version 2.
namespace OpenSage.Mathematics.FixedMath
{

    /// <summary>
    /// Represents a Q31.32 fixed-point number.
    /// </summary>
    public partial struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>
    {
        readonly long _rawValue;

        // Precision of this type is 2^-32, that is 2,3283064365386962890625E-10
        public static readonly decimal Precision = (decimal) (new Fix64(1L));//0.00000000023283064365386962890625m;
        public static readonly Fix64 MaxValue = new Fix64(MAX_VALUE);
        public static readonly Fix64 MinValue = new Fix64(MIN_VALUE);
        public static readonly Fix64 One = new Fix64(ONE);
        public static readonly Fix64 Zero = new Fix64();
        /// <summary>
        /// The value of Pi
        /// </summary>
        public static readonly Fix64 Pi = new Fix64(PI);
        public static readonly Fix64 PiOver2 = new Fix64(PI_OVER_2);
        public static readonly Fix64 PiTimes2 = new Fix64(PI_TIMES_2);
        public static readonly Fix64 PiInv = (Fix64) 0.3183098861837906715377675267M;
        public static readonly Fix64 PiOver2Inv = (Fix64) 0.6366197723675813430755350535M;
        static readonly Fix64 Log2Max = new Fix64(LOG2MAX);
        static readonly Fix64 Log2Min = new Fix64(LOG2MIN);
        static readonly Fix64 Ln2 = new Fix64(LN2);

        static readonly Fix64 LutInterval = (Fix64) (LUT_SIZE - 1) / PiOver2;
        const long MAX_VALUE = long.MaxValue;
        const long MIN_VALUE = long.MinValue;
        const int NUM_BITS = 64;
        const int FRACTIONAL_PLACES = 32;
        const long ONE = 1L << FRACTIONAL_PLACES;
        const long PI_TIMES_2 = 0x6487ED511;
        const long PI = 0x3243F6A88;
        const long PI_OVER_2 = 0x1921FB544;
        const long LN2 = 0xB17217F7;
        const long LOG2MAX = 0x1F00000000;
        const long LOG2MIN = -0x2000000000;
        const int LUT_SIZE = (int) (PI_OVER_2 >> 15);

        /// <summary>
        /// Returns a number indicating the sign of a Fix64 number.
        /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
        /// </summary>
        public static int Sign(Fix64 value)
        {
            return
                value._rawValue < 0 ? -1 :
                value._rawValue > 0 ? 1 :
                0;
        }


        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// Note: Abs(Fix64.MinValue) == Fix64.MaxValue.
        /// </summary>
        public static Fix64 Abs(Fix64 value)
        {
            if (value._rawValue == MIN_VALUE)
            {
                return MaxValue;
            }

            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value._rawValue >> 63;
            return new Fix64((value._rawValue + mask) ^ mask);
        }

        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// FastAbs(Fix64.MinValue) is undefined.
        /// </summary>
        public static Fix64 FastAbs(Fix64 value)
        {
            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value._rawValue >> 63;
            return new Fix64((value._rawValue + mask) ^ mask);
        }


        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        public static Fix64 Floor(Fix64 value)
        {
            // Just zero out the fractional part
            return new Fix64((long) ((ulong) value._rawValue & 0xFFFFFFFF00000000));
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
        public static Fix64 Ceiling(Fix64 value)
        {
            var hasFractionalPart = (value._rawValue & 0x00000000FFFFFFFF) != 0;
            return hasFractionalPart ? Floor(value) + One : value;
        }

        /// <summary>
        /// Rounds a value to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
        public static Fix64 Round(Fix64 value)
        {
            var fractionalPart = value._rawValue & 0x00000000FFFFFFFF;
            var integralPart = Floor(value);
            if (fractionalPart < 0x80000000)
            {
                return integralPart;
            }
            if (fractionalPart > 0x80000000)
            {
                return integralPart + One;
            }
            // if number is halfway between two values, round to the nearest even number
            // this is the method used by System.Math.Round().
            return (integralPart._rawValue & ONE) == 0
                       ? integralPart
                       : integralPart + One;
        }

        /// <summary>
        /// Adds x and y. Performs saturating addition, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static Fix64 operator +(Fix64 x, Fix64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;
            var sum = xl + yl;
            // if signs of operands are equal and signs of sum and x are different
            if (((~(xl ^ yl) & (xl ^ sum)) & MIN_VALUE) != 0)
            {
                sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
            }
            return new Fix64(sum);
        }

        /// <summary>
        /// Adds x and y witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static Fix64 FastAdd(Fix64 x, Fix64 y)
        {
            return new Fix64(x._rawValue + y._rawValue);
        }

        /// <summary>
        /// Subtracts y from x. Performs saturating substraction, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static Fix64 operator -(Fix64 x, Fix64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;
            var diff = xl - yl;
            // if signs of operands are different and signs of sum and x are different
            if ((((xl ^ yl) & (xl ^ diff)) & MIN_VALUE) != 0)
            {
                diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
            }
            return new Fix64(diff);
        }

        /// <summary>
        /// Subtracts y from x witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static Fix64 FastSub(Fix64 x, Fix64 y)
        {
            return new Fix64(x._rawValue - y._rawValue);
        }

        static long AddOverflowHelper(long x, long y, ref bool overflow)
        {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
            return sum;
        }

        public static Fix64 operator *(Fix64 x, Fix64 y)
        {

            var xl = x._rawValue;
            var yl = y._rawValue;

            var xlo = (ulong) (xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong) (yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long) xlo * yhi;
            var hilo = xhi * (long) ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            bool overflow = false;
            var sum = AddOverflowHelper((long) loResult, midResult1, ref overflow);
            sum = AddOverflowHelper(sum, midResult2, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);

            bool opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

            // if signs of operands are equal and sign of result is negative,
            // then multiplication overflowed positively
            // the reverse is also true
            if (opSignsEqual)
            {
                if (sum < 0 || (overflow && xl > 0))
                {
                    return MaxValue;
                }
            }
            else
            {
                if (sum > 0)
                {
                    return MinValue;
                }
            }

            // if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
            // then this means the result overflowed.
            var topCarry = hihi >> FRACTIONAL_PLACES;
            if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/)
            {
                return opSignsEqual ? MaxValue : MinValue;
            }

            // If signs differ, both operands' magnitudes are greater than 1,
            // and the result is greater than the negative operand, then there was negative overflow.
            if (!opSignsEqual)
            {
                long posOp, negOp;
                if (xl > yl)
                {
                    posOp = xl;
                    negOp = yl;
                }
                else
                {
                    posOp = yl;
                    negOp = xl;
                }
                if (sum > negOp && negOp < -ONE && posOp > ONE)
                {
                    return MinValue;
                }
            }

            return new Fix64(sum);
        }

        /// <summary>
        /// Performs multiplication without checking for overflow.
        /// Useful for performance-critical code where the values are guaranteed not to cause overflow
        /// </summary>
        public static Fix64 FastMul(Fix64 x, Fix64 y)
        {

            var xl = x._rawValue;
            var yl = y._rawValue;

            var xlo = (ulong) (xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong) (yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long) xlo * yhi;
            var hilo = xhi * (long) ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << FRACTIONAL_PLACES;

            var sum = (long) loResult + midResult1 + midResult2 + hiResult;
            return new Fix64(sum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int CountLeadingZeroes(ulong x)
        {
            int result = 0;
            while ((x & 0xF000000000000000) == 0) { result += 4; x <<= 4; }
            while ((x & 0x8000000000000000) == 0) { result += 1; x <<= 1; }
            return result;
        }

        public static Fix64 operator /(Fix64 x, Fix64 y)
        {
            var xl = x._rawValue;
            var yl = y._rawValue;

            if (yl == 0)
            {
                throw new DivideByZeroException();
            }

            var remainder = (ulong) (xl >= 0 ? xl : -xl);
            var divider = (ulong) (yl >= 0 ? yl : -yl);
            var quotient = 0UL;
            var bitPos = NUM_BITS / 2 + 1;


            // If the divider is divisible by 2^n, take advantage of it.
            while ((divider & 0xF) == 0 && bitPos >= 4)
            {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0)
            {
                int shift = CountLeadingZeroes(remainder);
                if (shift > bitPos)
                {
                    shift = bitPos;
                }
                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

                // Detect overflow
                if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
                {
                    return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // rounding
            ++quotient;
            var result = (long) (quotient >> 1);
            if (((xl ^ yl) & MIN_VALUE) != 0)
            {
                result = -result;
            }

            return new Fix64(result);
        }

        public static Fix64 operator %(Fix64 x, Fix64 y)
        {
            return new Fix64(
                x._rawValue == MIN_VALUE & y._rawValue == -1 ?
                0 :
                x._rawValue % y._rawValue);
        }

        /// <summary>
        /// Performs modulo as fast as possible; throws if x == MinValue and y == -1.
        /// Use the operator (%) for a more reliable but slower modulo.
        /// </summary>
        public static Fix64 FastMod(Fix64 x, Fix64 y)
        {
            return new Fix64(x._rawValue % y._rawValue);
        }

        public static Fix64 operator -(Fix64 x)
        {
            return x._rawValue == MIN_VALUE ? MaxValue : new Fix64(-x._rawValue);
        }

        public static bool operator ==(Fix64 x, Fix64 y)
        {
            return x._rawValue == y._rawValue;
        }

        public static bool operator !=(Fix64 x, Fix64 y)
        {
            return x._rawValue != y._rawValue;
        }

        public static bool operator >(Fix64 x, Fix64 y)
        {
            return x._rawValue > y._rawValue;
        }

        public static bool operator <(Fix64 x, Fix64 y)
        {
            return x._rawValue < y._rawValue;
        }

        public static bool operator >=(Fix64 x, Fix64 y)
        {
            return x._rawValue >= y._rawValue;
        }

        public static bool operator <=(Fix64 x, Fix64 y)
        {
            return x._rawValue <= y._rawValue;
        }

        /// <summary>
        /// Returns 2 raised to the specified power.
        /// Provides at least 6 decimals of accuracy.
        /// </summary>
        internal static Fix64 Pow2(Fix64 x)
        {
            if (x._rawValue == 0)
            {
                return One;
            }

            // Avoid negative arguments by exploiting that exp(-x) = 1/exp(x).
            bool neg = x._rawValue < 0;
            if (neg)
            {
                x = -x;
            }

            if (x == One)
            {
                return neg ? One / (Fix64) 2 : (Fix64) 2;
            }
            if (x >= Log2Max)
            {
                return neg ? One / MaxValue : MaxValue;
            }
            if (x <= Log2Min)
            {
                return neg ? MaxValue : Zero;
            }

            /* The algorithm is based on the power series for exp(x):
             * http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
             * 
             * From term n, we get term n+1 by multiplying with x/n.
             * When the sum term drops to zero, we can stop summing.
             */

            int integerPart = (int) Floor(x);
            // Take fractional part of exponent
            x = new Fix64(x._rawValue & 0x00000000FFFFFFFF);

            var result = One;
            var term = One;
            int i = 1;
            while (term._rawValue != 0)
            {
                term = FastMul(FastMul(x, term), Ln2) / (Fix64) i;
                result += term;
                i++;
            }

            result = FromRaw(result._rawValue << integerPart);
            if (neg)
            {
                result = One / result;
            }

            return result;
        }

        /// <summary>
        /// Returns the base-2 logarithm of a specified number.
        /// Provides at least 9 decimals of accuracy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        internal static Fix64 Log2(Fix64 x)
        {
            if (x._rawValue <= 0)
            {
                throw new ArgumentOutOfRangeException("Non-positive value passed to Ln", "x");
            }

            // This implementation is based on Clay. S. Turner's fast binary logarithm
            // algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
            //     Processing Mag., pp. 124,140, Sep. 2010.)

            long b = 1U << (FRACTIONAL_PLACES - 1);
            long y = 0;

            long rawX = x._rawValue;
            while (rawX < ONE)
            {
                rawX <<= 1;
                y -= ONE;
            }

            while (rawX >= (ONE << 1))
            {
                rawX >>= 1;
                y += ONE;
            }

            var z = new Fix64(rawX);

            for (int i = 0; i < FRACTIONAL_PLACES; i++)
            {
                z = FastMul(z, z);
                if (z._rawValue >= (ONE << 1))
                {
                    z = new Fix64(z._rawValue >> 1);
                    y += b;
                }
                b >>= 1;
            }

            return new Fix64(y);
        }

        /// <summary>
        /// Returns the natural logarithm of a specified number.
        /// Provides at least 7 decimals of accuracy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        public static Fix64 Ln(Fix64 x)
        {
            return FastMul(Log2(x), Ln2);
        }

        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// Provides about 5 digits of accuracy for the result.
        /// </summary>
        /// <exception cref="DivideByZeroException">
        /// The base was zero, with a negative exponent
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The base was negative, with a non-zero exponent
        /// </exception>
        public static Fix64 Pow(Fix64 b, Fix64 exp)
        {
            if (b == One)
            {
                return One;
            }
            if (exp._rawValue == 0)
            {
                return One;
            }
            if (b._rawValue == 0)
            {
                if (exp._rawValue < 0)
                {
                    throw new DivideByZeroException();
                }
                return Zero;
            }

            Fix64 log2 = Log2(b);
            return Pow2(exp * log2);
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was negative.
        /// </exception>
        public static Fix64 Sqrt(Fix64 x)
        {
            var xl = x._rawValue;
            if (xl < 0)
            {
                // We cannot represent infinities like Single and Double, and Sqrt is
                // mathematically undefined for x < 0. So we just throw an exception.
                throw new ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
            }

            var num = (ulong) xl;
            var result = 0UL;

            // second-to-top bit
            var bit = 1UL << (NUM_BITS - 2);

            while (bit > num)
            {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 128 bit values in computations.
            for (var i = 0; i < 2; ++i)
            {
                // First we get the top 48 bits of the answer.
                while (bit != 0)
                {
                    if (num >= result + bit)
                    {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else
                    {
                        result = result >> 1;
                    }
                    bit >>= 2;
                }

                if (i == 0)
                {
                    // Then process it again to get the lowest 16 bits.
                    if (num > (1UL << (NUM_BITS / 2)) - 1)
                    {
                        // The remainder 'num' is too large to be shifted left
                        // by 32, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (NUM_BITS / 2)) - 0x80000000UL;
                        result = (result << (NUM_BITS / 2)) + 0x80000000UL;
                    }
                    else
                    {
                        num <<= (NUM_BITS / 2);
                        result <<= (NUM_BITS / 2);
                    }

                    bit = 1UL << (NUM_BITS / 2 - 2);
                }
            }
            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result)
            {
                ++result;
            }
            return new Fix64((long) result);
        }

        /// <summary>
        /// Returns the Sine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static Fix64 Sin(Fix64 x)
        {
            var clampedL = ClampSinValue(x._rawValue, out var flipHorizontal, out var flipVertical);
            var clamped = new Fix64(clampedL);

            // Find the two closest values in the LUT and perform linear interpolation
            // This is what kills the performance of this function on x86 - x64 is fine though
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = FastSub(rawIndex, roundedIndex);

            var nearestValue = new Fix64(SinLut[flipHorizontal ?
                SinLut.Length - 1 - (int) roundedIndex :
                (int) roundedIndex]);
            var secondNearestValue = new Fix64(SinLut[flipHorizontal ?
                SinLut.Length - 1 - (int) roundedIndex - Sign(indexError) :
                (int) roundedIndex + Sign(indexError)]);

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue)))._rawValue;
            var interpolatedValue = nearestValue._rawValue + (flipHorizontal ? -delta : delta);
            var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;
            return new Fix64(finalValue);
        }

        /// <summary>
        /// Returns a rough approximation of the Sine of x.
        /// This is at least 3 times faster than Sin() on x86 and slightly faster than Math.Sin(),
        /// however its accuracy is limited to 4-5 decimals, for small enough values of x.
        /// </summary>
        public static Fix64 FastSin(Fix64 x)
        {
            var clampedL = ClampSinValue(x._rawValue, out bool flipHorizontal, out bool flipVertical);

            // Here we use the fact that the SinLut table has a number of entries
            // equal to (PI_OVER_2 >> 15) to use the angle to index directly into it
            var rawIndex = (uint) (clampedL >> 15);
            if (rawIndex >= LUT_SIZE)
            {
                rawIndex = LUT_SIZE - 1;
            }
            var nearestValue = SinLut[flipHorizontal ?
                SinLut.Length - 1 - (int) rawIndex :
                (int) rawIndex];
            return new Fix64(flipVertical ? -nearestValue : nearestValue);
        }


        static long ClampSinValue(long angle, out bool flipHorizontal, out bool flipVertical)
        {
            var largePI = 7244019458077122842;
            // Obtained from ((Fix64)1686629713.065252369824872831112M).m_rawValue
            // This is (2^29)*PI, where 29 is the largest N such that (2^N)*PI < MaxValue.
            // The idea is that this number contains way more precision than PI_TIMES_2,
            // and (((x % (2^29*PI)) % (2^28*PI)) % ... (2^1*PI) = x % (2 * PI)
            // In practice this gives us an error of about 1,25e-9 in the worst case scenario (Sin(MaxValue))
            // Whereas simply doing x % PI_TIMES_2 is the 2e-3 range.

            var clamped2Pi = angle;
            for (int i = 0; i < 29; ++i)
            {
                clamped2Pi %= (largePI >> i);
            }
            if (angle < 0)
            {
                clamped2Pi += PI_TIMES_2;
            }

            // The LUT contains values for 0 - PiOver2; every other value must be obtained by
            // vertical or horizontal mirroring
            flipVertical = clamped2Pi >= PI;
            // obtain (angle % PI) from (angle % 2PI) - much faster than doing another modulo
            var clampedPi = clamped2Pi;
            while (clampedPi >= PI)
            {
                clampedPi -= PI;
            }
            flipHorizontal = clampedPi >= PI_OVER_2;
            // obtain (angle % PI_OVER_2) from (angle % PI) - much faster than doing another modulo
            var clampedPiOver2 = clampedPi;
            if (clampedPiOver2 >= PI_OVER_2)
            {
                clampedPiOver2 -= PI_OVER_2;
            }
            return clampedPiOver2;
        }

        /// <summary>
        /// Returns the cosine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static Fix64 Cos(Fix64 x)
        {
            var xl = x._rawValue;
            var rawAngle = xl + (xl > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
            return Sin(new Fix64(rawAngle));
        }

        /// <summary>
        /// Returns a rough approximation of the cosine of x.
        /// See FastSin for more details.
        /// </summary>
        public static Fix64 FastCos(Fix64 x)
        {
            var xl = x._rawValue;
            var rawAngle = xl + (xl > 0 ? -PI - PI_OVER_2 : PI_OVER_2);
            return FastSin(new Fix64(rawAngle));
        }

        /// <summary>
        /// Returns the tangent of x.
        /// </summary>
        /// <remarks>
        /// This function is not well-tested. It may be wildly inaccurate.
        /// </remarks>
        public static Fix64 Tan(Fix64 x)
        {
            var clampedPi = x._rawValue % PI;
            var flip = false;
            if (clampedPi < 0)
            {
                clampedPi = -clampedPi;
                flip = true;
            }
            if (clampedPi > PI_OVER_2)
            {
                flip = !flip;
                clampedPi = PI_OVER_2 - (clampedPi - PI_OVER_2);
            }

            var clamped = new Fix64(clampedPi);

            // Find the two closest values in the LUT and perform linear interpolation
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = Round(rawIndex);
            var indexError = FastSub(rawIndex, roundedIndex);

            var nearestValue = new Fix64(TanLut[(int) roundedIndex]);
            var secondNearestValue = new Fix64(TanLut[(int) roundedIndex + Sign(indexError)]);

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue)))._rawValue;
            var interpolatedValue = nearestValue._rawValue + delta;
            var finalValue = flip ? -interpolatedValue : interpolatedValue;
            return new Fix64(finalValue);
        }

        /// <summary>
        /// Returns the arccos of of the specified number, calculated using Atan and Sqrt
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static Fix64 Acos(Fix64 x)
        {
            if (x < -One || x > One)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (x._rawValue == 0) return PiOver2;

            var result = Atan(Sqrt(One - x * x) / x);
            return x._rawValue < 0 ? result + Pi : result;
        }

        /// <summary>
        /// Returns the arctan of of the specified number, calculated using Euler series
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static Fix64 Atan(Fix64 z)
        {
            if (z._rawValue == 0) return Zero;

            // Force positive values for argument
            // Atan(-z) = -Atan(z).
            var neg = z._rawValue < 0;
            if (neg)
            {
                z = -z;
            }

            Fix64 result;
            var two = (Fix64) 2;
            var three = (Fix64) 3;

            bool invert = z > One;
            if (invert) z = One / z;

            result = One;
            var term = One;

            var zSq = z * z;
            var zSq2 = zSq * two;
            var zSqPlusOne = zSq + One;
            var zSq12 = zSqPlusOne * two;
            var dividend = zSq2;
            var divisor = zSqPlusOne * three;

            for (var i = 2; i < 30; ++i)
            {
                term *= dividend / divisor;
                result += term;

                dividend += zSq2;
                divisor += zSq12;

                if (term._rawValue == 0) break;
            }

            result = result * z / zSqPlusOne;

            if (invert)
            {
                result = PiOver2 - result;
            }

            if (neg)
            {
                result = -result;
            }
            return result;
        }

        public static Fix64 Atan2(Fix64 y, Fix64 x)
        {
            var yl = y._rawValue;
            var xl = x._rawValue;
            if (xl == 0)
            {
                if (yl > 0)
                {
                    return PiOver2;
                }
                if (yl == 0)
                {
                    return Zero;
                }
                return -PiOver2;
            }
            Fix64 atan;
            var z = y / x;

            // Deal with overflow
            if (One + (Fix64) 0.28M * z * z == MaxValue)
            {
                return y < Zero ? -PiOver2 : PiOver2;
            }

            if (Abs(z) < One)
            {
                atan = z / (One + (Fix64) 0.28M * z * z);
                if (xl < 0)
                {
                    if (yl < 0)
                    {
                        return atan - Pi;
                    }
                    return atan + Pi;
                }
            }
            else
            {
                atan = PiOver2 - z / (z * z + (Fix64) 0.28M);
                if (yl < 0)
                {
                    return atan - Pi;
                }
            }
            return atan;
        }



        public static explicit operator Fix64(long value)
        {
            return new Fix64(value * ONE);
        }
        public static explicit operator long(Fix64 value)
        {
            return value._rawValue >> FRACTIONAL_PLACES;
        }
        public static explicit operator Fix64(float value)
        {
            return new Fix64((long) (value * ONE));
        }
        public static explicit operator float(Fix64 value)
        {
            return (float) value._rawValue / ONE;
        }
        public static explicit operator Fix64(double value)
        {
            return new Fix64((long) (value * ONE));
        }
        public static explicit operator double(Fix64 value)
        {
            return (double) value._rawValue / ONE;
        }
        public static explicit operator Fix64(decimal value)
        {
            return new Fix64((long) (value * ONE));
        }
        public static explicit operator decimal(Fix64 value)
        {
            return (decimal) value._rawValue / ONE;
        }

        public override bool Equals(object obj)
        {
            return obj is Fix64 && ((Fix64) obj)._rawValue == _rawValue;
        }

        public override int GetHashCode()
        {
            return _rawValue.GetHashCode();
        }

        public bool Equals(Fix64 other)
        {
            return _rawValue == other._rawValue;
        }

        public int CompareTo(Fix64 other)
        {
            return _rawValue.CompareTo(other._rawValue);
        }

        public override string ToString()
        {
            // Up to 10 decimal places
            return ((decimal) this).ToString("0.##########");
        }

        public static Fix64 FromRaw(long rawValue)
        {
            return new Fix64(rawValue);
        }

        internal static void GenerateSinLut()
        {
            using (var writer = new StreamWriter("Fix64SinLut.cs"))
            {
                writer.Write(
@"namespace FixMath.NET 
{
    partial struct Fix64 
    {
        public static readonly long[] SinLut = new[] 
        {");
                int lineCounter = 0;
                for (int i = 0; i < LUT_SIZE; ++i)
                {
                    var angle = i * Math.PI * 0.5 / (LUT_SIZE - 1);
                    if (lineCounter++ % 8 == 0)
                    {
                        writer.WriteLine();
                        writer.Write("            ");
                    }
                    var sin = Math.Sin(angle);
                    var rawValue = ((Fix64) sin)._rawValue;
                    writer.Write(string.Format("0x{0:X}L, ", rawValue));
                }
                writer.Write(
@"
        };
    }
}");
            }
        }

        internal static void GenerateTanLut()
        {
            using (var writer = new StreamWriter("Fix64TanLut.cs"))
            {
                writer.Write(
@"namespace FixMath.NET 
{
    partial struct Fix64 
    {
        public static readonly long[] TanLut = new[] 
        {");
                int lineCounter = 0;
                for (int i = 0; i < LUT_SIZE; ++i)
                {
                    var angle = i * Math.PI * 0.5 / (LUT_SIZE - 1);
                    if (lineCounter++ % 8 == 0)
                    {
                        writer.WriteLine();
                        writer.Write("            ");
                    }
                    var tan = Math.Tan(angle);
                    if (tan > (double) MaxValue || tan < 0.0)
                    {
                        tan = (double) MaxValue;
                    }
                    var rawValue = (((decimal) tan > (decimal) MaxValue || tan < 0.0) ? MaxValue : (Fix64) tan)._rawValue;
                    writer.Write(string.Format("0x{0:X}L, ", rawValue));
                }
                writer.Write(
@"
        };
    }
}");
            }
        }

        // turn into a Console Application and use this to generate the look-up tables
        //static void Main(string[] args)
        //{
        //    GenerateSinLut();
        //    GenerateTanLut();
        //}

        /// <summary>
        /// The underlying integer representation
        /// </summary>
        public long RawValue => _rawValue;

        /// <summary>
        /// This is the constructor from raw value; it can only be used interally.
        /// </summary>
        /// <param name="rawValue"></param>
        Fix64(long rawValue)
        {
            _rawValue = rawValue;
        }

        public Fix64(int value)
        {
            _rawValue = value * ONE;
        }
    }
}
