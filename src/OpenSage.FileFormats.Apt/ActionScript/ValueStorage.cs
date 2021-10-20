using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenSage.FileFormats.Apt.ActionScript
{
    public enum RawParamType
    {
        UI8,

        UI16,
        UI24,
        UI32,

        I16,
        I32,
        
        Jump8,
        Jump16,
        Jump32,
        Jump64,

        Float,
        Double,
        Boolean, 
        String,

        ArrayBegin,
        ArrayEnd,
        ArraySize,

        BranchOffset, 

        Constant,
        Register, 
    }
    public enum RawValueType
    {
        String = 0,
        Boolean = 1,
        Integer = 2,
        Float = 3,

        Constant = 4,
        Register = 5,
    }

    public class ValueStorage
    {
        public RawValueType Type { get; private set; }

        public string String { get; private set; }
        public bool Boolean { get; private set; }
        public int Number { get; private set; }
        public double Decimal { get; private set; }

        public override string ToString()
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

        public static ValueStorage Parse(string str)
        {
            if (!str.StartsWith('(') || !str.EndsWith(')') || str.IndexOf(';') < 0)
                throw new InvalidOperationException();
            var c1 = str.IndexOf(';');
            if (!int.TryParse(str.Substring(1, c1 - 1), out var tint))
                throw new InvalidOperationException();
            var t = (RawValueType) tint;
            var ans = new ValueStorage() { Type = t };
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
                        throw new InvalidOperationException();
                    else
                        ans.Number = n;
                    break;
                case RawValueType.Float:
                    if (!double.TryParse(content, out var d))
                        throw new InvalidOperationException();
                    else
                        ans.Decimal = d;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return ans;
        }

        public static ValueStorage FromRegister(uint num)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Register;
            v.Number = (int) num;
            return v;
        }

        public static ValueStorage FromConstant(uint id)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Constant;
            v.Number = (int) id;
            return v;
        }

        public ValueStorage ToRegister()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromRegister((uint) Number);
        }

        public ValueStorage ToConstant()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromConstant((uint) Number);
        }

        public static ValueStorage FromBoolean(bool cond)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Boolean;
            v.Boolean = cond;
            return v;
        }

        public static ValueStorage FromString(string str)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.String;
            v.String = str;
            return v;
        }

        public static ValueStorage FromInteger(int num)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Integer;
            v.Number = num;
            return v;
        }

        // TODO is it okay?
        public static ValueStorage FromUInteger(uint num)
        {
            var v = new ValueStorage();
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

        public static ValueStorage FromFloat(double num)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Float;
            v.Decimal = num;
            return v;
        }

    }
}


