using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAS2.Base
{

    public class RawValue
    {
        public RawValueType Type { get; internal set; }

        public string String { get; internal set; }
        public bool Boolean { get; internal set; }
        public int Number { get; internal set; }
        public double Decimal { get; internal set; }

        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            string ans = string.Empty;
            switch (Type) {
                case RawValueType.String:
                    ans = JsonSerializer.Serialize(String);
                    break;
                case RawValueType.Boolean:
                    ans = Boolean ? "true" : "false";
                    break;
                case RawValueType.Integer:
                case RawValueType.Constant:
                case RawValueType.Register:
                    ans = Number.ToString();
                    break;
                case RawValueType.Float:
                    ans = Decimal.ToString();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return $"({(int) Type};{ans})";
        }

        public static RawValue Deserialize(string str)
        {
            if (!str.StartsWith('(') || !str.EndsWith(')') || str.IndexOf(';') < 0)
                throw new InvalidDataException();
            var c1 = str.IndexOf(';');
            if (!int.TryParse(str.Substring(1, c1 - 1), out var tint))
                throw new InvalidDataException();
            var t = (RawValueType) tint;
            var ans = new RawValue() { Type = t };
            var content = str.Substring(c1 + 1, str.Length - 2 - c1);
            switch (t)
            {
                case RawValueType.String:
                    ans.String = JsonSerializer.Deserialize<string>(content);
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
                        ans.Number = n;
                    break;
                case RawValueType.Float:
                    if (!double.TryParse(content, out var d))
                        throw new InvalidDataException();
                    else
                        ans.Decimal = d;
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
            v.Number = (int) num;
            return v;
        }

        public static RawValue FromConstant(uint id)
        {
            var v = new RawValue();
            v.Type = RawValueType.Constant;
            v.Number = (int) id;
            return v;
        }

        public RawValue ToRegister()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromRegister((uint) Number);
        }

        public RawValue ToConstant()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromConstant((uint) Number);
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
            v.Number = num;
            return v;
        }

        // TODO is it okay?
        public static RawValue FromUInteger(uint num)
        {
            var v = new RawValue();
            if (num > 0x0FFFFFFF)
            {
                v.Type = RawValueType.Float;
                v.Decimal = (double) num;
            }
            else
            {
                v.Type = RawValueType.Integer;
                v.Number = (int) num;
            }
            return v;
        }

        public static RawValue FromFloat(double num)
        {
            var v = new RawValue();
            v.Type = RawValueType.Float;
            v.Decimal = num;
            return v;
        }

    }
}


