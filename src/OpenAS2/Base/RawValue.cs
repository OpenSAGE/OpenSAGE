using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenAS2.Base
{

    public class ValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RawValue);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var str = (string)reader.Value!;
            return RawValue.Deserialize(str);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            RawValue val2 = (RawValue)value!;
            writer.WriteValue(val2.Serialize());
        }
    }

    [JsonConverter(typeof(ValueConverter))]
    public class RawValue
    {
        public RawValueType Type { get; internal set; }

        public string String { get; internal set; } = string.Empty;
        public bool Boolean { get; internal set; }
        public int Integer { get; internal set; }
        public double Double { get; internal set; }

        public override string ToString() { return ToString(null, null); }
        public string ToString(IList<ConstantEntry>? cp = null, RawInstruction? cas = null)
        {
            string ans = "";
            switch (Type)
            {
                case RawValueType.String:
                    ans = String.ToCodingForm();
                    break;
                case RawValueType.Boolean:
                    ans = Boolean ? "true" : "false";
                    break;
                case RawValueType.Integer:
                    ans = Integer.ToString();
                    break;
                case RawValueType.Constant:
                    ans = (cp != null && cas != null) ? cp[cas.Parameters[Integer + 1].Integer].Value.ToString()! : ("C" + Integer.ToString());
                    break;
                case RawValueType.Register:
                    ans = "R" + Integer.ToString();
                    break;
                case RawValueType.Float:
                    ans = Double.ToString() + "D";
                    break;
                default:
                    throw new NotImplementedException();
            }
            return ans;
        }

        public string Serialize()
        {
            string ans = string.Empty;
            switch (Type)
            {
                case RawValueType.String:
                    ans = JsonConvert.SerializeObject(String);
                    break;
                case RawValueType.Boolean:
                    ans = Boolean ? "true" : "false";
                    break;
                case RawValueType.Integer:
                case RawValueType.Constant:
                case RawValueType.Register:
                    ans = Integer.ToString();
                    break;
                case RawValueType.Float:
                    ans = Double.ToString();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return $"({(int)Type};{ans})";
        }

        public static RawValue Deserialize(string str)
        {
            if (!str.StartsWith('(') || !str.EndsWith(')') || str.IndexOf(';') < 0)
                throw new InvalidDataException();
            var c1 = str.IndexOf(';');
            if (!int.TryParse(str.Substring(1, c1 - 1), out var tint))
                throw new InvalidDataException();
            var t = (RawValueType)tint;
            var ans = new RawValue() { Type = t };
            var content = str.Substring(c1 + 1, str.Length - 2 - c1);
            switch (t)
            {
                case RawValueType.String:
                    ans.String = JsonConvert.DeserializeObject<string>(content) ?? string.Empty;
                    break;
                case RawValueType.Boolean:
                    ans.Boolean = content.StartsWith("true");
                    break;
                case RawValueType.Integer:
                case RawValueType.Constant:
                case RawValueType.Register:
                    if (!int.TryParse(content, out var n))
                        throw new InvalidDataException();
                    else
                        ans.Integer = n;
                    break;
                case RawValueType.Float:
                    if (!double.TryParse(content, out var d))
                        throw new InvalidDataException();
                    else
                        ans.Double = d;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return ans;
        }

        public static RawValue FromRegister(uint num)
        {
            var v = new RawValue();
            v.Type = RawValueType.Register;
            v.Integer = (int)num;
            return v;
        }

        public static RawValue FromConstant(uint id)
        {
            var v = new RawValue();
            v.Type = RawValueType.Constant;
            v.Integer = (int)id;
            return v;
        }

        public RawValue ToRegister()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromRegister((uint)Integer);
        }

        public RawValue ToConstant()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromConstant((uint)Integer);
        }

        public static RawValue FromBoolean(bool cond)
        {
            var v = new RawValue();
            v.Type = RawValueType.Boolean;
            v.Boolean = cond;
            return v;
        }

        public static RawValue FromString(string str)
        {
            var v = new RawValue();
            v.Type = RawValueType.String;
            v.String = str;
            return v;
        }

        public static RawValue FromInteger(int num)
        {
            var v = new RawValue();
            v.Type = RawValueType.Integer;
            v.Integer = num;
            return v;
        }

        // TODO is it okay?
        public static RawValue FromUInteger(uint num)
        {
            var v = new RawValue();
            if (num > 0x0FFFFFFF)
            {
                v.Type = RawValueType.Float;
                v.Double = (double)num;
            }
            else
            {
                v.Type = RawValueType.Integer;
                v.Integer = (int)num;
            }
            return v;
        }

        public static RawValue FromFloat(double num)
        {
            var v = new RawValue();
            v.Type = RawValueType.Float;
            v.Double = num;
            return v;
        }

    }
}


