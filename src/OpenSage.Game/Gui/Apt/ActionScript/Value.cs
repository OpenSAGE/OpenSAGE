using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Gui.Apt.ActionScript
{
    public enum ValueType
    {
        String,
        Constant,
        Integer,
        Short,
        Float,
        Undefined
    }

    public struct Value
    {
        public ValueType Type { get; set; }

        public string String;
        public int Number;
        public double Decimal;

        public static Value FromConstant(uint id)
        {
            var v = new Value();
            v.Type = ValueType.Constant;
            v.Number = (int)id;
            return v;
        }

        public static Value FromString(string str)
        {
            var v = new Value();
            v.Type = ValueType.String;
            v.String = str;
            return v;
        }

        public static Value FromInteger(int num)
        {
            var v = new Value();
            v.Type = ValueType.Integer;
            v.Number = num;
            return v;
        }

        public static Value FromFloat(float num)
        {
            var v = new Value();
            v.Type = ValueType.Float;
            v.Decimal = num;
            return v;
        }

        public static Value Undefined()
        {
            var v = new Value();
            v.Type = ValueType.Undefined;
            return v;
        }
    }
}
