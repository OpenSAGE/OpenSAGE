using System;
using System.IO;

namespace OpenSage.Data.Utilities
{
    internal static class EnumUtility
    {
        public static TEnum CastValueAsEnum<TValue, TEnum>(TValue value)
           where TEnum : struct
        {
            var enumValue = (TEnum) Enum.ToObject(typeof(TEnum), value);
            if (!Enum.IsDefined(typeof(TEnum), enumValue))
            {
                throw new InvalidDataException($"Unexpected value for {typeof(TEnum).Name}: {value}");
            }

            return enumValue;
        }
    }
}
