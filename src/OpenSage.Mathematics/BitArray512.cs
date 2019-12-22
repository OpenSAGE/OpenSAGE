using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace OpenSage.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BitArray512
    {
        // We use individual fields instead of an array to avoid an extra allocation.
        // Fixed size buffers are only usable in unsafe structs.
        private ulong _a0;
        private ulong _a1;
        private ulong _a2;
        private ulong _a3;
        private ulong _a4;
        private ulong _a5;
        private ulong _a6;
        private ulong _a7;

        /// <summary>
        /// Lazily computed number of 1 bits. -1 is a special value, which indicates the cache is invalid.
        /// </summary>
        private int _setBits;

        public readonly int Length { get; }

        /// <summary>
        /// Is any bit set to 1?
        /// </summary>
        public bool AnyBitSet
        {
            get => NumBitsSet > 0;
        }

        /// <summary>
        /// NUmber of 1 bits in this array.
        /// </summary>
        public int NumBitsSet
        {
            get
             {
                // Refresh the cache if required.
                if (_setBits == -1)
                {
                    _setBits = CountSetBits();
                }

                return _setBits;
            }
        }

        public BitArray512(int length) : this()
        {
            if (length < 0 || length > 512)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    $"Length must between 0 and 512."
                );
            }

            Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int bit, bool value)
        {
            if (bit < 0 || bit >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(bit));
            }

            var offset = bit >> 6; // bit / 64
            var mask = (ulong) 1 << bit;

            unsafe
            {
                var pointer = (ulong*) Unsafe.AsPointer(ref this);
                if (value)
                {
                    *(pointer + offset) |= mask;
                }
                else
                {
                    *(pointer + offset) &= (byte) ~mask;
                } 
            }

            // Mark cache as invalid.
            _setBits = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int bit)
        {
            if (bit < 0 || bit >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(bit));
            }

            var offset = bit >> 6; // bit / 64
            var mask = (ulong) 1 << bit;

            unsafe
            {
                var pointer = (ulong*) Unsafe.AsPointer(ref this);
                return (pointer[offset] & mask) != 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Clear()
        {
            _a0 = 0;
            _a1 = 0;
            _a2 = 0;
            _a3 = 0;
            _a4 = 0;
            _a5 = 0;
            _a6 = 0;
            _a7 = 0;
            _setBits = 0;
        }

        public void SetAll(bool value)
        {
            // If we're clearing the bits, just set all the fields to 0.
            if (!value)
            {
                Clear();
                return;
            }

            // However, if we're setting the bits to 1 we can't just set all the fields to ulong.Maxvalue,
            // because otherwise we would get 1 bits outside of the actual length of the array,
            // which would be incorrectly counted by CountSetBits().

            var byteOffset = 0;
            var remainingBits = Length;

            unsafe
            {
                var fieldsPointer = (byte*) Unsafe.AsPointer(ref this);

                // Set as many bits at a time as you can.

                while (remainingBits >= 64)
                {
                    *(ulong*)(fieldsPointer + byteOffset) = ulong.MaxValue;
                    byteOffset += 8;
                    remainingBits -= 64;
                }

                if (remainingBits >= 32)
                {
                    *(uint*)(fieldsPointer + byteOffset) = uint.MaxValue;
                    byteOffset += 4;
                    remainingBits -= 32;
                }

                if (remainingBits >= 16)
                {
                    *(ushort*)(fieldsPointer + byteOffset) = ushort.MaxValue;
                    byteOffset += 2;
                    remainingBits -= 16;
                }

                if (remainingBits >= 8)
                {
                    *(fieldsPointer + byteOffset) = byte.MaxValue;
                    byteOffset += 1;
                    remainingBits -= 8;
                }

                // This is a mask consisting of `remainingBits´ ones.
                var remainingMask = (1 << remainingBits) - 1;
                *(fieldsPointer + byteOffset) |= (byte) remainingMask;
            }

            _setBits = Length;
        }

        public void CopyFrom(in BitArray512 other)
        {
            if (Length != other.Length)
            {
                throw new ArgumentException(nameof(other), "Both BitArrays must have the same length.");
            }

            _a0 = other._a0;
            _a1 = other._a1;
            _a2 = other._a2;
            _a3 = other._a3;
            _a4 = other._a4;
            _a5 = other._a5;
            _a6 = other._a6;
            _a7 = other._a7;
            _setBits = other._setBits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CountSetBits()
        {
            return BitOperations.PopCount(_a0) +
                   BitOperations.PopCount(_a1) +
                   BitOperations.PopCount(_a2) +
                   BitOperations.PopCount(_a3) +
                   BitOperations.PopCount(_a4) +
                   BitOperations.PopCount(_a5) +
                   BitOperations.PopCount(_a6) +
                   BitOperations.PopCount(_a7);
        }

        public BitArray512 And(in BitArray512 other)
        {
            return new BitArray512(Math.Max(Length, other.Length)) {
                _a0 = _a0 & other._a0,
                _a1 = _a1 & other._a1,
                _a2 = _a2 & other._a2,
                _a3 = _a3 & other._a3,
                _a4 = _a4 & other._a4,
                _a5 = _a5 & other._a5,
                _a6 = _a6 & other._a6,
                _a7 = _a7 & other._a7,
                _setBits = -1
            };
        }

        public bool Equals(in BitArray512 other)
        {
            return
                Length == other.Length &&
                _a0 == other._a0 &&
                _a1 == other._a1 &&
                _a2 == other._a2 &&
                _a3 == other._a3 &&
                _a4 == other._a4 &&
                _a5 == other._a5 &&
                _a6 == other._a6 &&
                _a7 == other._a7;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_a0, _a1, _a2, _a3, _a4, _a5, _a6, _a7);
        }
    }
}
