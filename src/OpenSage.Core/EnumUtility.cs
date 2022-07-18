using System;
using System.IO;

namespace OpenSage.FileFormats
{
    public static class EnumUtility
    {
        public static TEnum CastValueAsEnum<TValue, TEnum>(TValue value)
           where TValue : struct
           where TEnum : struct
        {
            var enumValue = (TEnum) Enum.ToObject(typeof(TEnum), value);
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

        public static int GetEnumCount<TEnum>()
            where TEnum : Enum
        {
            return Enum.GetNames(typeof(TEnum)).Length;
        }
    }
}
