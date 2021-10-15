using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.FileFormats.Apt.ActionScript;
using OpenSage.FileFormats;
using OpenSage.Gui.Apt.ActionScript.Library;

namespace OpenSage.Gui.Apt.ActionScript
{
    public enum ValueType
    {
        String,
        Boolean,
        Integer,
        Short,
        Float,
        Object,
        Undefined,

        Constant,
        Register,
        Return,
    }

    public class Value
    {
        public ValueType Type { get; private set; }

        public string DisplayString { get; set; }

        private string _string;
        private bool _boolean;
        private int _number;
        private double _decimal;
        private ObjectContext _object;
        private ActionContext _actx;

        public bool IsNumericType()
        {
            return Type == ValueType.Float || Type == ValueType.Integer;
        }

        public bool Enumerable()
        {
            // TODO try to implement although not necessary
            return false;
        }

        public Value ResolveRegister(ActionContext context)
        {
            if (Type != ValueType.Register)
                return this;

            var result = this;
            if (_number < context.RegisterCount && context.RegisterStored(_number))
            {
                var entry = context.GetRegister(_number);
                result = entry;
            }
            return result;
        }

        public Value ResolveConstant(ActionContext context)
        {
            if (Type != ValueType.Constant)
                return this;

            var result = this;
            if (_number < context.Constants.Count) {
                var entry = context.Constants[_number];
                result = entry;
            }
            return result;
        }

        public Value ResolveReturn()
        {
            if (Type != ValueType.Return)
                return this;
            return _actx.Return ? _actx.ReturnValue : Undefined();
        }

        public Value GetReturnValue()
        {
            if (Type != ValueType.Return)
                return this;
            return _actx.Return ? _actx.ReturnValue : null;
        }

        internal Value ToNumber()
        {
            if (IsNumericType())
                return Type == ValueType.Float ? Value.FromFloat(_decimal) : Value.FromInteger(_number);
            else
                return Value.FromInteger(0);
        }

        public static Value FromFunction(Function func)
        {
            var v = new Value();
            v.Type = ValueType.Object;
            v._object = func;
            return v;
        }

        public static Value FromObject(ObjectContext obj)
        {
            if (obj != null && obj.IsFunction()) return FromFunction((Function) obj);
            var v = new Value();
            v.Type = ValueType.Object;
            v._object = obj;
            return v;
        }

        public static Value FromArray(Value[] array, VM vm)
        {
            var v = new Value();
            v.Type = ValueType.Object;
            v._object = new ASArray(array, vm);
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

        public static Value ReturnValue(ActionContext actx)
        {
            var v = new Value();
            v.Type = ValueType.Return;
            v._actx = actx;
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

        // TODO is it okay?
        public static Value FromUInteger(uint num)
        {
            var v = new Value();
            if (num > 0x0FFFFFFF)
            {
                v.Type = ValueType.Float;
                v._decimal = (double) num;
            }
            else
            {
                v.Type = ValueType.Integer;
                v._number = (int) num;
            }
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

        public static Value FromStorage(ValueStorage s)
        {
            switch (s.Type)
            {
                case RawValueType.String:
                    return FromString(s.String);
                case RawValueType.Integer:
                    return FromInteger(s.Number);
                case RawValueType.Float:
                    return FromFloat(s.Decimal);
                case RawValueType.Boolean:
                    return FromBoolean(s.Boolean);
                case RawValueType.Constant:
                    return FromConstant((uint) s.Number);
                case RawValueType.Register:
                    return FromRegister((uint) s.Number);
                default:
                    throw new InvalidOperationException("Well...This situation is really weird to be reached.");
            }
        }

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public T ToObject<T>() where T : ObjectContext
        {
            if (Type == ValueType.Undefined)
            {
                logger.Error("Cannot create object from undefined!");
                return null;
            }
            if (Type != ValueType.Object)
                throw new InvalidOperationException();

            return (T) _object;
        }

        public ObjectContext ToObject()
        {
            if (Type == ValueType.Undefined)
            {
                logger.Error("Cannot create object from undefined!");
                return null;
            }

            if (Type == ValueType.String)
            {
                return new ASString(this, null);
            }

            if (Type != ValueType.Object)
                throw new InvalidOperationException();

            return _object;
        }

        // Follow ECMA specification 9.4: https://www.ecma-international.org/ecma-262/5.1/#sec-9.4
        // TODO optimize if possible
        public int ToInteger()
        {
            if (Type == ValueType.Constant || Type == ValueType.Register)
                return _number;

            double floatNumber = ToFloat();
            if (double.IsNaN(floatNumber))
            {
                return 0;
            }

            return Math.Sign(floatNumber) * (int) Math.Abs(floatNumber);
        }

        public uint ToUInteger()
        {
            double number = ToFloat();
            if (double.IsNaN(number) || double.IsInfinity(number)) return 0;
            double posInt = Math.Sign(number) * (int) Math.Abs(number);
            uint ans = (uint) (posInt % 0x10000000);
            return ans;
        }

        /// Used by AptEditor to get actual id of constant / register
        public uint GetIDValue()
        {
            if (Type != ValueType.Constant && Type != ValueType.Register)
                throw new InvalidOperationException();

            return (uint) _number;
        }

        // TODO: implement integer conversion etc.
        public double ToReal()
        {
            if (Type == ValueType.Float) {
                return _decimal;
            }

            // TODO
            throw new NotImplementedException();
        }

        public bool ToBoolean()
        {
            bool var;
            switch (Type)
            {
                case ValueType.String:
                    var = _string.Length > 0;
                    break;
                case ValueType.Object:
                    var = _object == null;
                    break;
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
            if (Type == ValueType.Undefined)
            {
                logger.Error("Undefined Function!");
                return null;
            }
            if (Type != ValueType.Object || _object is not Function)
                throw new InvalidOperationException();

            return (Function) _object;
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
                case ValueType.Constant:
                case ValueType.Register:
                    return _number.ToString();
                case ValueType.Float:
                    return _decimal.ToString();
                case ValueType.Undefined:
                    return "undefined"; // follows ECMA-262
                case ValueType.Object:
                    return _object == null ? "null" : _object.ToString();
                case ValueType.Return:
                    return _actx.ReturnValue == null ? "undefined" : _actx.ReturnValue.ToString();
                default:
                    throw new NotImplementedException(Type.ToString());
            }
        }

        public string GetStringType()
        {
            string result = null;
            switch (Type)
            {
                case ValueType.String:
                    result = "string";
                    break;
                case ValueType.Boolean:
                    result = "boolean";
                    break;
                case ValueType.Integer:
                case ValueType.Short:
                case ValueType.Float:
                    result = "number";
                    break;
                case ValueType.Object:
                    if (_object is MovieClip)
                        result = "movieclip";
                    else if (_object is Function)
                        result = "function";
                    else
                        result = "object";
                    break;
                case ValueType.Undefined:
                    result = "undefined";
                    break;
                default:
                    throw new InvalidOperationException(Type.ToString());
            }
            return result;
        }

        public string ToStringWithType(ActionContext ctx)
        {
            var ttype = "?";
            try { ttype = this.Type.ToString().Substring(0, 3); }
            catch (InvalidOperationException e) { }
            string tstr = DisplayString;
            if (tstr == null || tstr == "")
            {
                if (this.Type == ValueType.Constant && ctx != null) tstr = this.ResolveConstant(ctx).ToString();
                else if (this.Type == ValueType.Register && ctx != null) tstr = this.ResolveRegister(ctx).ToString();
                else if (this.Type == ValueType.Return) tstr = this.ResolveReturn().ToString();
                else if (this.Type == ValueType.Object) tstr = this.ToString();
                else tstr = this.ToString();
            }
            return $"({ttype}){tstr}";
            }

        // Follow ECMA specification 9.3: https://www.ecma-international.org/ecma-262/5.1/#sec-9.3
        public double ToFloat()
        {
            switch (Type)
            {
                case ValueType.Constant:
                case ValueType.Register:
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
