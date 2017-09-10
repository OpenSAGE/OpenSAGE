using System;
using System.IO;

namespace OpenSage.Data.Utilities
{
    internal static class EnumUtility
    {
        public static TEnum CastValueAsEnum<TValue, TEnum>(TValue value)
           where TEnum : struct
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                throw new InvalidDataException($"Unexpected value for {typeof(TEnum).Name}: {value}");
            }

            return (TEnum) (object) value;
        }
    }
}
