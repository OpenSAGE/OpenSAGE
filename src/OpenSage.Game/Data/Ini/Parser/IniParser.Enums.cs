using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSage.LowLevel.Input;

namespace OpenSage.Data.Ini.Parser
{
    partial class IniParser
    {
        private static readonly Dictionary<Type, Dictionary<string, Enum>> CachedEnumMap = new Dictionary<Type, Dictionary<string, Enum>>();

        static IniParser()
        {
            CachedEnumMap.Add(typeof(Key), new Dictionary<string, Enum>
            {
                { "KEY_NONE", Key.None },
                { "KEY_0", Key.D0 },
                { "KEY_1", Key.D1 },
                { "KEY_2", Key.D2 },
                { "KEY_3", Key.D3 },
                { "KEY_4", Key.D4 },
                { "KEY_5", Key.D5 },
                { "KEY_6", Key.D6 },
                { "KEY_7", Key.D7 },
                { "KEY_8", Key.D8 },
                { "KEY_9", Key.D9 },
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
                { "KEY_LBRACKET", Key.LeftBracket },
                { "KEY_RBRACKET", Key.RightBracket },
                { "KEY_COMMA", Key.Comma },
                { "KEY_PERIOD", Key.Period },
                { "KEY_BACKSLASH", Key.Backslash },
                { "KEY_SLASH", Key.Slash },
                { "KEY_SPACE", Key.Space },
                { "KEY_ENTER", Key.Enter },
                { "KEY_TAB", Key.Tab },
                { "KEY_DEL", Key.Delete },
                { "KEY_ESC", Key.Escape },
                { "KEY_TICK", Key.Tick },
                { "KEY_BACKSPACE", Key.Backspace },
                { "KEY_MINUS", Key.Minus },
                { "KEY_EQUAL", Key.Equal },
                { "KEY_KP0", Key.NumPad0 },
                { "KEY_KP1", Key.NumPad1 },
                { "KEY_KP2", Key.NumPad2 },
                { "KEY_KP3", Key.NumPad3 },
                { "KEY_KP4", Key.NumPad4 },
                { "KEY_KP5", Key.NumPad5 },
                { "KEY_KP6", Key.NumPad6 },
                { "KEY_KP7", Key.NumPad7 },
                { "KEY_KP8", Key.NumPad8 },
                { "KEY_KP9", Key.NumPad9 },
                { "KEY_KPSLASH", Key.NumPadDivide },
            });
        }

        public static Dictionary<string, Enum> GetEnumMap<T>()
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
