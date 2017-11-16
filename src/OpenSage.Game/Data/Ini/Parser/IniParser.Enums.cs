using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenSage.Data.Ini.Parser
{
    partial class IniParser
    {
        private static readonly Dictionary<Type, Dictionary<string, Enum>> CachedEnumMap = new Dictionary<Type, Dictionary<string, Enum>>();

        private static Dictionary<string, Enum> GetEnumMap<T>()
            where T : struct
        {
            var enumType = typeof(T);
            if (!CachedEnumMap.TryGetValue(enumType, out var stringToValueMap))
            {
                lock (CachedEnumMap)
                {
                    stringToValueMap = Enum.GetValues(enumType)
                        .Cast<Enum>()
                        .Distinct()
                        .SelectMany(x => GetIniNames(enumType, x).Select(y => new { Name = y, Value = x }))
                        .Where(x => x.Name != null)
                        .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

                    // It might have been added by another thread.
                    if (!CachedEnumMap.ContainsKey(enumType))
                    {
                        CachedEnumMap.Add(enumType, stringToValueMap);
                    }
                }
            }
            return stringToValueMap;
        }

        private T ParseEnum<T>(Dictionary<string, T> stringToValueMap)
            where T : struct
        {
            var token = GetNextToken();

            if (stringToValueMap.TryGetValue(token.Text.ToUpper(), out var enumValue))
                return enumValue;

            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.Text}'", token.Position);
        }

        public T ParseEnum<T>()
            where T : struct
        {
            return ParseEnum<T>(GetNextToken());
        }

        public static T ParseEnum<T>(IniToken token)
            where T : struct
        {
            var stringToValueMap = GetEnumMap<T>();

            if (stringToValueMap.TryGetValue(token.Text.ToUpper(), out var enumValue))
                return (T)(object)enumValue;

            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.Text}'", token.Position);
        }

        public T ParseEnumFlags<T>()
            where T : struct
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = (T) (object) 0;

            IniToken? token;
            while ((token = GetNextTokenOptional()) != null)
            {
                if (!stringToValueMap.TryGetValue(token.Value.Text.ToUpper(), out var enumValue))
                    throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.Value.Text}'", token.Value.Position);

                // Ugh.
                result = (T) (object) ((int) (object) result | (int) (object) enumValue);
            }

            return result;
        }

        public BitArray<T> ParseEnumBitArray<T>()
            where T : struct
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = new BitArray<T>();

            IniToken? token;
            while ((token = GetNextTokenOptional()) != null)
            {
                var stringValue = token.Value.Text.ToUpper();
                switch (stringValue)
                {
                    case "ALL":
                        result.SetAll(true);
                        break;

                    case "NONE":
                        result.SetAll(false);
                        break;

                    default:
                        var value = true;
                        
                        if (stringValue.StartsWith("-") || stringValue.StartsWith("+"))
                        {
                            value = stringValue[0] == '+';
                            stringValue = stringValue.Substring(1);
                        }
                        if (!stringToValueMap.TryGetValue(stringValue, out var enumValue))
                        {
                            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{stringValue}'", token.Value.Position);
                        }

                        // Ugh.
                        result.Set((T)(object)enumValue, true);

                        break;
                }
            }

            return result;
        }

        private static string[] GetIniNames(Type enumType, Enum value)
        {
            var field = enumType.GetTypeInfo().GetDeclaredField(value.ToString());
            var iniEnumAttribute = field.GetCustomAttribute<IniEnumAttribute>();
            return iniEnumAttribute?.Names ?? new string[0];
        }
    }
}
