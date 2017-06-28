using System;
using System.Reflection;

namespace OpenZH.Data.Utilities.Extensions
{
    internal static class DecoderExtensions
    {
        public static uint DecodeValue(this uint token, byte start, byte end)
        {
            uint mask = GenerateMask(start, end);
            int shift = start;

            return (token & mask) >> shift;
        }

        public static T DecodeValue<T>(this uint token, byte start, byte end)
            where T : struct
        {
            var decodedValue = DecodeValue(token, start, end);
            if (typeof(T).GetTypeInfo().IsEnum)
                return (T) Enum.ToObject(typeof(T), decodedValue);
            return (T) Convert.ChangeType(decodedValue, typeof(T));
        }

        private static uint GenerateMask(byte start, byte end)
        {
            uint mask = 0;
            for (int i = start; i <= end; i++)
                mask |= (uint) Math.Pow(2, i);
            return mask;
        }

        public static string ToFourCcString(this uint fourCc)
        {
            var a = fourCc.DecodeValue<char>(00, 07);
            var b = fourCc.DecodeValue<char>(08, 15);
            var c = fourCc.DecodeValue<char>(16, 23);
            var d = fourCc.DecodeValue<char>(24, 31);

            return new string(new[] { a, b, c, d });
        }
    }
}
