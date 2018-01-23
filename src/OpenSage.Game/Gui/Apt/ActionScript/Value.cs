using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt;

namespace OpenSage.Gui.Apt.ActionScript
{
    public enum ValueType
    {
        String,
        Constant,
        Integer,
        Short,
        Float,
        Object,
        Undefined
    }

    public class Value
    {
        public ValueType Type { get; set; }

        public string String;
        public int Number;
        public double Decimal;
        public ObjectContext Object;

        public void Resolve(ActionContext context)
        {
            if (Type != ValueType.Constant)
                throw new InvalidOperationException();

            var entry = context.Apt.Constants.Entries[Number];
            switch (entry.Type)
            {
                case ConstantEntryType.String:
                    Type = ValueType.String;
                    String = (string) entry.Value;
                    break;
                case ConstantEntryType.Integer:
                    Type = ValueType.Integer;
                    Number = (int) entry.Value;
                    break;
            }
        }

        public static Value FromObject(ObjectContext obj)
        {
            var v = new Value();
            v.Type = ValueType.Object;
            v.Object = obj;
            return v;
        }

        public static Value FromConstant(uint id)
        {
            var v = new Value();
            v.Type = ValueType.Constant;
            v.Number = (int) id;
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

        public ObjectContext ToObject()
        {
            if (Type != ValueType.Object)
                throw new InvalidOperationException();

            return Object;
        }

        public int ToInteger()
        {
            if (Type != ValueType.Integer && Type != ValueType.Constant)
                throw new InvalidOperationException();

            return Number;
        }

        public string ToString()
        {
            if (Type != ValueType.String)
                throw new InvalidOperationException();

            return String;
        }
    }
}
