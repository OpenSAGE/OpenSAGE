using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace OpenAS2.Base
{
    public class ConstantEntryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ConstantEntry);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var str = (string)reader.Value!;
            return ConstantEntry.Deserialize(str);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ConstantEntry val2 = (ConstantEntry)value!;
            writer.WriteValue(val2.Serialize());
        }
    }

    [JsonConverter(typeof(ConstantEntryConverter))]
    public sealed class ConstantEntry
    {

        public ConstantType Type { get; internal set; }
        public object Value { get; internal set; }

        public override string ToString()
        {
            return $"{Type}:{Value}";
        }

        public string Serialize()
        {
            string ans = string.Empty;
            switch (Type)
            {
                case ConstantType.Undef:
                    ans = "undefined";
                    break;
                case ConstantType.None:
                    ans = "null";
                    break;
                case ConstantType.String:
                    ans = ((string)Value).ToCodingForm();
                    break;
                case ConstantType.Boolean:
                    ans = ((bool)Value) ? "true" : "false";
                    break;
                case ConstantType.Integer:
                    ans = ((int)Value).ToString();
                    break;
                case ConstantType.Lookup:
                case ConstantType.Register:
                    ans = ((uint)Value).ToString();
                    break;
                case ConstantType.Float:
                    ans = ((float)Value).ToString();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return $"({(int)Type};{ans})";
        }

        public static ConstantEntry Deserialize(string str)
        {
            if (!str.StartsWith('(') || !str.EndsWith(')') || str.IndexOf(';') < 0)
                throw new InvalidDataException();
            var c1 = str.IndexOf(';');
            if (!int.TryParse(str.Substring(1, c1 - 1), out var tint))
                throw new InvalidDataException();
            var t = (ConstantType)tint;
            var ans = new ConstantEntry() { Type = t };
            var content = str.Substring(c1 + 1, str.Length - 2 - c1);
            switch (t)
            {
                case ConstantType.Undef:
                    throw new InvalidDataException("Undefined const entry");
                case ConstantType.String:
                    ans.Value = JsonConvert.DeserializeObject<string>(content) ?? content; //  throw new InvalidOperationException();
                    break;
                case ConstantType.Boolean:
                    ans.Value = content.StartsWith("true");
                    break;
                case ConstantType.Float:
                    if (!float.TryParse(content, out var d))
                        throw new InvalidDataException();
                    else
                        ans.Value = d;
                    break;
                case ConstantType.Integer:
                    if (!int.TryParse(content, out var n))
                        throw new InvalidDataException();
                    else
                        ans.Value = n;
                    break;
                case ConstantType.Register:
                case ConstantType.Lookup:
                    if (!uint.TryParse(content, out var n1))
                        throw new InvalidDataException();
                    else
                        ans.Value = n1;
                    break;
                case ConstantType.None:
                    ans.Value = null;
                    break;
                default:
                    throw new InvalidDataException();
            }
            return ans;
        }
    }
}
