using System;
using OpenSage.Data.Apt;
using OpenSage.FileFormats;
using OpenSage.Gui.Apt.ActionScript.Library;

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
        Array,
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
        private Value[] _array;

        public bool IsNumericType()
        {
            return Type == ValueType.Float || Type == ValueType.Integer;
        }

        public Value ResolveRegister(ActionContext context)
        {
            if (Type != ValueType.Register)
                return this;

            if (context.Registers.Length - 1 < _number)
                return FromInteger(_number);

            return context.Registers[_number];
        }

        public Value ResolveConstant(ActionContext context)
        {
            if (Type != ValueType.Constant)
                return this;

            Value result;

            var entry = context.Constants[_number];
            switch (entry.Type)
            {
                case ConstantEntryType.String:
                    result = FromString((string) entry.Value);
                    break;
                case ConstantEntryType.Register:
                    result = FromRegister((uint) entry.Value);
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

        public static Value FromArray(Value[] array)
        {
            var v = new Value();
            v.Type = ValueType.Array;
            v._array = array;
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

        public static Value FromFloat(double num)
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


        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ObjectContext ToObject()
        {
            if (Type == ValueType.Undefined)
            {
                logger.Error("Cannot create object from undefined!");
                return null;
            }

            if (Type == ValueType.String)
            {
                return new ASString(_string);
            }

            if (Type != ValueType.Object)
                throw new InvalidOperationException();

            return _object;
        }

        // Follow ECMA specification 9.4: https://www.ecma-international.org/ecma-262/5.1/#sec-9.4
        public int ToInteger()
        {
            double floatNumber = ToFloat();

            if (double.IsNaN(floatNumber))
            {
                return 0;
            }

            return Math.Sign(floatNumber) * (int) Math.Abs(floatNumber);
        }

        /// Used by AptEditor to get actual id of constant / register
        public uint GetIDValue()
        {
            if(Type != ValueType.Constant && Type != ValueType.Register)
                throw new InvalidOperationException();
            
            return (uint)_number;
        }

        // TODO: implement integer conversion etc.
        public double ToReal()
        {
            if(Type == ValueType.Float) {
                return _decimal;
            }

            // TODO
            throw new NotImplementedException();
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

        // Follow ECMA specification 9.8: https://www.ecma-international.org/ecma-262/5.1/#sec-9.8
        public override string ToString()
        {
            switch (Type)
            {
                case ValueType.String:
                    return _string;
                case ValueType.Boolean:
                    return _boolean.ToString();
                case ValueType.Short:
                case ValueType.Integer:
                    return _number.ToString();
                case ValueType.Float:
                    return _decimal.ToString();
                case ValueType.Undefined:
                    return "";
                case ValueType.Object:
                    return _object.Item.Name;
                default:
                    throw new NotImplementedException();
            }
        }

        // Follow ECMA specification 9.3: https://www.ecma-international.org/ecma-262/5.1/#sec-9.3
        public double ToFloat()
        {
            switch (Type)
            {
                case ValueType.Constant:
                case ValueType.Short:
                case ValueType.Integer:
                    return _number;
                case ValueType.Float:
                    return _decimal;
                case ValueType.Undefined:
                    return float.NaN;
                case ValueType.Boolean:
                    return _boolean ? 1.0 : 0.0;
                default:
                    throw new NotImplementedException();
            }
        }

        public TEnum ToEnum<TEnum>() where TEnum : struct
        {
            if (Type != ValueType.Integer)
                throw new InvalidOperationException();

            return EnumUtility.CastValueAsEnum<int, TEnum>(_number);
        }

        //TODO: According to page 74, ActionEquals2 of SWF7 format specification
        //http://www.prowiki.org/upload/HelmutLeitner/flash_file_format_specification.pdf
        //Here an Abstract Equality Comparison Algorithm should be implemented.
        //(Section 11.9.3, ECMAScript Specification 3)
        //https://www-archive.mozilla.org/js/language/E262-3.pdf
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
                case ValueType.String:
                    result = (b._string == _string);
                    break;
                case ValueType.Boolean:
                    result = b._boolean == _boolean;
                    break;
                case ValueType.Object:
                    result = b._object == _object;
                    break;
                case ValueType.Integer:
                    result = b._number == _number;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public bool StrictEquals(Value b)
        {
            bool result;

            if (Type != b.Type)
                return false;

            //TODO: Implement Strict Equality Comparison Algorithm
            switch (Type)
            {
                case ValueType.Undefined:
                    result = true;
                    break;
                case ValueType.String:
                    result = (b._string == _string);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }
    }
}
