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
        Short
    }

    public struct Value
    {
        public ValueType Type { get; set; }

        public string String;
        public int Number;
        public double Decimal;

        public static Value Constant(uint id)
        {
            var v = new Value();
            v.Type = ValueType.Constant;
            v.Number = (int)id;
            return v;
        }


    }
}
