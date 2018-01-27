using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Boolean,
        Integer,
        Register,
        Short,
        Float,
        Object,
        Function,
        Undefined
    }

    public class Value
    {
        public ValueType Type { get; set; }

        private string _string;
        private bool _boolean;
        private int _number;
        private double _decimal;
        private ObjectContext _object;
        private Function _function;

        public Value ResolveRegister(ActionContext context)
        {
            if (Type != ValueType.Register)
                return this;

            return context.Registers[_number];
        }

        public Value ResolveConstant(ActionContext context)
        {
            if (Type != ValueType.Constant)
                throw new InvalidOperationException();

            Value result;

            var entry = context.Apt.Constants.Entries[_number];
            switch (entry.Type)
            {
                case ConstantEntryType.String:
                    result = Value.FromString((string) entry.Value);
                    break;
                case ConstantEntryType.Register:
                    result = Value.FromRegister((uint) entry.Value);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public static Value FromFunction(Function func)
        {
            var v = new Value();
            v.Type = ValueType.Function;
            v._function = func;
            return v;
        }

        public static Value FromObject(ObjectContext obj)
        {
            var v = new Value();
            v.Type = ValueType.Object;
            v._object = obj;
            return v;
        }


        public static Value FromRegister(uint num)
        {
            var v = new Value();
            v.Type = ValueType.Register;
            v._number = (int) num;
            return v;
        }

        public static Value FromConstant(uint id)
        {
            var v = new Value();
            v.Type = ValueType.Constant;
            v._number = (int) id;
            return v;
        }

        public static Value FromBoolean(bool cond)
        {
            var v = new Value();
            v.Type = ValueType.Boolean;
            v._boolean = cond;
            return v;
        }

        public static Value FromString(string str)
        {
            var v = new Value();
            v.Type = ValueType.String;
            v._string = str;
            return v;
        }

        public static Value FromInteger(int num)
        {
            var v = new Value();
            v.Type = ValueType.Integer;
            v._number = num;
            return v;
        }

        public static Value FromFloat(float num)
        {
            var v = new Value();
            v.Type = ValueType.Float;
            v._decimal = num;
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
            if(Type ==ValueType.Undefined)
            {
                Debug.WriteLine("[ERROR] cannot convert to object!");
                return null;
            }

            if (Type != ValueType.Object)
                throw new InvalidOperationException();

            return _object;
        }

        public int ToInteger()
        {
            if (Type != ValueType.Integer && Type != ValueType.Constant)
                throw new InvalidOperationException();

            return _number;
        }

        public bool ToBoolean()
        {
            if (Type == ValueType.String || Type == ValueType.Object)
                throw new InvalidOperationException();

            bool var;

            switch (Type)
            {
                case ValueType.Boolean:
                    var = _boolean;
                    break;
                case ValueType.Undefined:
                    var = false;
                    break;
                case ValueType.Float:
                    var = (_decimal != 0);
                    break;
                case ValueType.Integer:
                case ValueType.Short:
                    var = (_number != 0);
                    break;
                default:
                    throw new InvalidOperationException();
            }


            return var;
        }

        public Function ToFunction()
        {
            if (Type != ValueType.Function)
                throw new InvalidOperationException();

            return _function;
        }

        public override string ToString()
        {
            if (Type != ValueType.String &&
                Type != ValueType.Undefined)
                throw new InvalidOperationException();

            return _string;
        }

        public bool Equals(Value b)
        {
            bool result;

            if (Type != b.Type)
                return false;

            switch (Type)
            {
                case ValueType.Undefined:
                    result = true;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }
    }
}
