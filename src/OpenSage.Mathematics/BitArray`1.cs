﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OpenSage.Mathematics;

public sealed class BitArray<TEnum> : IEquatable<BitArray<TEnum>>
    where TEnum : Enum
{
    public static BitArray<TEnum> CreateAllSet()
    {
        var result = new BitArray<TEnum>();
        result.SetAll(true);
        return result;
    }

    private BitArray512 _data;

    public bool AnyBitSet => _data.AnyBitSet;
    public int NumBitsSet => _data.NumBitsSet;

    public bool BitsChanged { get; set; } = true;

    public BitArray()
    {
        var maxBits = GetNumValues();
        if (maxBits >= 512)
        {
            throw new Exception($"Cannot create a BitArray for enum {typeof(TEnum).Name}, because it has {maxBits} cases (max 512).");
        }
        _data = new BitArray512(maxBits);
    }

    /// <summary>
    /// Constructs a new bit array with the provided values set to true.
    /// </summary>
    /// <param name="values"></param>
    public BitArray(params TEnum[] values) : this()
    {
        foreach (var value in values)
        {
            Set(value, true);
        }
    }

    public BitArray(System.Collections.BitArray bitArray)
    {
        if (bitArray.Length >= 512)
        {
            throw new ArgumentException($"Cannot construct BitArray512 from a BitArray of length {bitArray.Length}.");
        }

        _data = new BitArray512(bitArray.Length);

        for (var i = 0; i < bitArray.Length; i++)
        {
            _data.Set(i, bitArray[i]);
        }
    }

    public BitArray(BitArray<TEnum> bitArray)
        : this()
    {
        for (var i = 0; i < _data.Length; i++)
        {
            _data.Set(i, bitArray._data.Get(i));
        }
    }

    public BitArray(in BitArray512 bitArray)
    {
        _data = bitArray;
    }

    public bool Get(int bit)
    {
        return _data.Get(bit);
    }

    public bool Get(TEnum bit)
    {
        // This avoids an object allocation.
        var bitI = Unsafe.As<TEnum, int>(ref bit);
        return Get(bitI);
    }

    public void Set(int bit, bool value)
    {
        BitsChanged |= _data.Get(bit) != value;
        _data.Set(bit, value);
    }

    public void Set(TEnum bit, bool value)
    {
        // This avoids an object allocation.
        var bitI = Unsafe.As<TEnum, int>(ref bit);
        Set(bitI, value);
    }

    public void SetAll(bool value)
    {
        _data.SetAll(value);
        BitsChanged = true;
    }

    public void CopyFrom(BitArray<TEnum> other)
    {
        _data.CopyFrom(other._data);
        BitsChanged = true;
    }

    public int CountIntersectionBits(BitArray<TEnum> other)
    {
        return _data.And(other._data).NumBitsSet;
    }

    public bool Intersects(BitArray<TEnum> other)
    {
        return CountIntersectionBits(other) > 0;
    }

    public IEnumerable<TEnum> GetSetBits()
    {
        for (var i = 0; i < _data.Length; i++)
        {
            if (_data.Get(i))
            {
                yield return (TEnum)(object)i;
            }
        }
    }

    public static BitArray<TEnum> operator |(BitArray<TEnum> left, BitArray<TEnum> right)
    {
        return new BitArray<TEnum>(left._data.Or(right._data));
    }

    public static BitArray<TEnum> operator &(BitArray<TEnum> left, BitArray<TEnum> right)
    {
        return new BitArray<TEnum>(left._data.And(right._data));
    }

    public string DisplayName
    {
        get
        {
            var result = string.Empty;

            foreach (var bit in GetSetBits())
            {
                result += bit.ToString() + ", ";
            }

            return (result == string.Empty)
                ? "(None)"
                : result.Trim(' ', ',');
        }
    }

    public bool Equals(BitArray<TEnum>? other) => _data.Equals(other?._data);

    public override int GetHashCode()
    {
        return _data.GetHashCode();
    }

    public BitArray<TEnum> Clone()
    {
        var result = new BitArray<TEnum>();
        result.CopyFrom(this);
        return result;
    }

    private static readonly ConcurrentDictionary<Type, int> CachedNumValues = new();

    private static int GetNumValues()
    {
        var key = typeof(TEnum);
        return CachedNumValues.GetOrAdd(key, x => Enum.GetValues(x).Length);
    }
}
