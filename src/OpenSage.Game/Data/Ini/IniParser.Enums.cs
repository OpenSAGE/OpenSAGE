using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Data.Ini
{
    partial class IniParser
    {
        private static readonly Dictionary<Type, Dictionary<string, Enum>> CachedEnumMap = new Dictionary<Type, Dictionary<string, Enum>>();
        private static readonly Dictionary<Type, Dictionary<Enum, string>> CachedEnumMapReverse = new Dictionary<Type, Dictionary<Enum, string>>();

        static IniParser()
        {
            CachedEnumMap.Add(typeof(Key), new Dictionary<string, Enum>
            {
                { "KEY_NONE", Key.Unknown },
                { "KEY_0", Key.Number0 },
                { "KEY_1", Key.Number1 },
                { "KEY_2", Key.Number2 },
                { "KEY_3", Key.Number3 },
                { "KEY_4", Key.Number4 },
                { "KEY_5", Key.Number5 },
                { "KEY_6", Key.Number6 },
                { "KEY_7", Key.Number7 },
                { "KEY_8", Key.Number8 },
                { "KEY_9", Key.Number9 },
                { "KEY_F1", Key.F1 },
                { "KEY_F2", Key.F2 },
                { "KEY_F3", Key.F3 },
                { "KEY_F4", Key.F4 },
                { "KEY_F5", Key.F5 },
                { "KEY_F6", Key.F6 },
                { "KEY_F7", Key.F7 },
                { "KEY_F8", Key.F8 },
                { "KEY_F9", Key.F9 },
                { "KEY_F10", Key.F10 },
                { "KEY_F11", Key.F11 },
                { "KEY_F12", Key.F12 },
                { "KEY_LEFT", Key.Left },
                { "KEY_RIGHT", Key.Right },
                { "KEY_UP", Key.Up },
                { "KEY_DOWN", Key.Down },
                { "KEY_A", Key.A },
                { "KEY_B", Key.B },
                { "KEY_C", Key.C },
                { "KEY_D", Key.D },
                { "KEY_E", Key.E },
                { "KEY_F", Key.F },
                { "KEY_G", Key.G },
                { "KEY_H", Key.H },
                { "KEY_I", Key.I },
                { "KEY_J", Key.J },
                { "KEY_K", Key.K },
                { "KEY_L", Key.L },
                { "KEY_M", Key.M },
                { "KEY_N", Key.N },
                { "KEY_O", Key.O },
                { "KEY_P", Key.P },
                { "KEY_Q", Key.Q },
                { "KEY_R", Key.R },
                { "KEY_S", Key.S },
                { "KEY_T", Key.T },
                { "KEY_U", Key.U },
                { "KEY_V", Key.V },
                { "KEY_W", Key.W },
                { "KEY_X", Key.X },
                { "KEY_Y", Key.Y },
                { "KEY_Z", Key.Z },
                { "KEY_LBRACKET", Key.BracketLeft },
                { "KEY_RBRACKET", Key.BracketRight },
                { "KEY_COMMA", Key.Comma },
                { "KEY_PERIOD", Key.Period },
                { "KEY_BACKSLASH", Key.BackSlash },
                { "KEY_SLASH", Key.Slash },
                { "KEY_SPACE", Key.Space },
                { "KEY_ENTER", Key.Enter },
                { "KEY_TAB", Key.Tab },
                { "KEY_DEL", Key.Delete },
                { "KEY_ESC", Key.Escape },
                { "KEY_TICK", Key.Grave },
                { "KEY_BACKSPACE", Key.BackSpace },
                { "KEY_MINUS", Key.Minus },
                { "KEY_EQUAL", Key.Plus },
                { "KEY_KP0", Key.Keypad0 },
                { "KEY_KP1", Key.Keypad1 },
                { "KEY_KP2", Key.Keypad2 },
                { "KEY_KP3", Key.Keypad3 },
                { "KEY_KP4", Key.Keypad4 },
                { "KEY_KP5", Key.Keypad5 },
                { "KEY_KP6", Key.Keypad6 },
                { "KEY_KP7", Key.Keypad7 },
                { "KEY_KP8", Key.Keypad8 },
                { "KEY_KP9", Key.Keypad9 },
                { "KEY_KPSLASH", Key.KeypadDivide },
            });

            CachedEnumMap.Add(typeof(DistributionType), new Dictionary<string, Enum>
            {
                { "CONSTANT", DistributionType.Constant },
                { "UNIFORM", DistributionType.Uniform },
            });
        }

        public static Dictionary<string, Enum> GetEnumMap<T>()
            where T : Enum
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

        public static Dictionary<Enum, string> GetEnumMapReverse<T>()
            where T : Enum
        {
            var enumType = typeof(T);
            if (!CachedEnumMapReverse.TryGetValue(enumType, out var valueToStringMap))
            {
                lock (CachedEnumMapReverse)
                {
                    valueToStringMap = Enum.GetValues(enumType)
                        .Cast<Enum>()
                        .Distinct()
                        .SelectMany(x => GetIniNames(enumType, x).Select(y => new { Name = y, Value = x }))
                        .Where(x => x.Name != null)
                        .ToDictionary(x => x.Value, x => x.Name);

                    // It might have been added by another thread.
                    if (!CachedEnumMapReverse.ContainsKey(enumType))
                    {
                        CachedEnumMapReverse.Add(enumType, valueToStringMap);
                    }
                }
            }
            return valueToStringMap;
        }

        private T ParseEnum<T>(Dictionary<string, T> stringToValueMap)
            where T : struct
        {
            var token = GetNextToken();

            if (stringToValueMap.TryGetValue(token.Text.ToUpperInvariant(), out var enumValue))
            {
                return enumValue;
            }

            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.Text}'", token.Position);
        }

        public T ParseEnum<T>()
            where T : Enum
        {
            return ScanEnum<T>(GetNextToken());
        }

        public static T ScanEnum<T>(in IniToken token)
            where T : Enum
        {
            var stringToValueMap = GetEnumMap<T>();

            if (stringToValueMap.TryGetValue(token.Text.ToUpperInvariant(), out var enumValue))
            {
                return (T) enumValue;
            }

            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.Text}'", token.Position);
        }

        public static T ParseEnum<T>(string value)
            where T : Enum
        {
            var stringToValueMap = GetEnumMap<T>();

            if (stringToValueMap.TryGetValue(value.ToUpperInvariant(), out var enumValue))
            {
                return (T) enumValue;
            }
            return default;
        }

        public T ParseEnumFlags<T>()
            where T : Enum
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = (T) (object) 0;

            IniToken? token;
            while ((token = GetNextTokenOptional()) != null)
            {
                var stringValue = token.Value.Text.ToUpperInvariant();
                switch (stringValue)
                {
                    case "NONE":
                        result = (T)(object)0;
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
                        if (value)
                        {
                            result = (T)(object)((int)(object)result | (int)(object)enumValue);
                        }
                        else
                        {
                            result = (T)(object)((int)(object)result & ~(int)(object)enumValue);
                        }

                        break;
                }
            }

            return result;
        }

        public BitArray<T> ParseInLineEnumBitArray<T>()
            where T : Enum
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = new BitArray<T>();

            IniToken? token;
            while ((token = PeekNextTokenOptional(SeparatorsColon)).HasValue)
            {
                var value = token.Value.Text.ToUpperInvariant();
                if (!ParseBitValue(stringToValueMap, result, value, inLine: true))
                {
                    return result;
                }

                GetNextTokenOptional(SeparatorsColon); //to proceed
            }

            return result;
        }

        public BitArray<T> ParseEnumBitArray<T>()
            where T : Enum
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = new BitArray<T>();

            IniToken? token;
            while ((token = GetNextTokenOptional()) != null)
            {
                var stringValue = token.Value.Text.ToUpperInvariant();
                ParseBitValue(stringToValueMap, result, stringValue);
            }

            return result;
        }

        public BitArray<T> ParseEnumBitArray<T>(string valuesString)
            where T : Enum
        {
            return ParseEnumBitArray<T>(valuesString, CurrentPosition);
        }

        public static BitArray<T> ParseEnumBitArray<T>(string valuesString, in IniTokenPosition currentPosition)
            where T : Enum
        {
            var stringToValueMap = GetEnumMap<T>();

            var result = new BitArray<T>();

            var values = valuesString.Trim().Replace("\"", "").Split(' ');
            for(var i = 0; i < values.Length; i++)
            {
                var stringValue = values[i];
                ParseBitValue(stringToValueMap, result, stringValue, currentPosition);
            }

            return result;
        }

        private bool ParseBitValue<T>(Dictionary<string, Enum> stringToValueMap, BitArray<T> result, string stringValue, bool inLine = false)
            where T : Enum
        {
            return ParseBitValue(stringToValueMap, result, stringValue, CurrentPosition, inLine);
        }

        private static bool ParseBitValue<T>(Dictionary<string, Enum> stringToValueMap, BitArray<T> result, string stringValue, in IniTokenPosition currentPosition, bool inLine = false)
            where T : Enum
        {
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
                        if (inLine)
                        {
                            return false;
                        }
                        else
                        {
                            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{stringValue}'", currentPosition);
                        }
                    }

                    // Ugh.
                    result.Set((T) enumValue, value);

                    break;
            }
            return true;
        }

        private static string[] GetIniNames(Type enumType, Enum value)
        {
            var field = enumType.GetTypeInfo().GetDeclaredField(value.ToString());
            var iniEnumAttribute = field.GetCustomAttribute<IniEnumAttribute>();
            return iniEnumAttribute?.Names ?? new string[0];
        }
    }
}
