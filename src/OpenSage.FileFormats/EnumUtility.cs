using System;
using System.IO;
using System.Linq;

namespace OpenSage.FileFormats;

public static class EnumUtility
{
    public static TEnum CastValueAsEnum<TValue, TEnum>(TValue value)
       where TValue : struct
       where TEnum : struct
    {
        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value);
        if (!IsValueDefined(enumValue))
        {
            throw new InvalidDataException($"Unexpected value for {typeof(TEnum).Name}: {value}");
        }

        return enumValue;
    }

    public static bool IsValueDefined<TEnum>(TEnum value)
        where TEnum : struct
    {
        // TODO: Is it faster to cache this?
        return Enum.IsDefined(typeof(TEnum), value);
    }

    /// <summary>
    /// Returns the size of the array that is needed if you want to be able to
    /// use the enum values as integer indices into the array. Note that this
    /// is subtely different from the number of items in the enum; for example
    /// enum values can be repeated, or there could be a non-contiguous range
    /// of values.
    /// </summary>
    public static int GetEnumValueLength<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetValuesAsUnderlyingType<TEnum>().Cast<int>().Max() + 1;
    }
}
