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
        Null, 
        Undefined, 

        Boolean,
        Integer,
        Float,

        String,
        Object,

        Constant,
        Register,
        Return,
    }

    public class Value
    {
        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public readonly ValueType Type;

        private readonly string _string;
        private readonly bool _boolean;
        private readonly int _number;
        private readonly double _decimal;
        private readonly ASObject _object;
        private readonly ExecutionContext _actx;

        public string DisplayString { get; set; }

        protected Value(ValueType type,
            string s = null,
            bool b = false,
            int n = 0,
            double d = 0,
            ASObject o = null,
            ExecutionContext a = null)
        {
            Type = type;
            _string = s;
            _boolean = b;
            _number = n;
            _decimal = d;
            _object = o;
            _actx = a;
        }

        // judgement

        public bool IsSpecialType() { return Type == ValueType.Constant || Type == ValueType.Register || Type == ValueType.Return; }

        public static bool IsUndefined(Value v) { return v != null && v.Type == ValueType.Undefined; }
        public static bool IsNull(Value v) { return v == null || (v.Type == ValueType.Null); }
        public static bool IsNumber(Value v) { return v != null && v.IsNumber(); }
        public static bool IsString(Value v) { return v != null && v.IsString(); }

        public bool IsUndefined() { return Type == ValueType.Undefined; }
        public bool IsNull() { return Type == ValueType.Null; }
        public bool IsNumber() { return Type == ValueType.Float || Type == ValueType.Integer; }
        public bool IsString() { return Type == ValueType.String || (Type == ValueType.Object && _object is ASString); }

        public static bool IsCallable(Value v) { return v != null && (v._object as ASFunction) != null; }
        public static bool IsPrimitive(Value v) { return IsNull(v) || v.Type != ValueType.Object; }

        public bool IsCallable() { return IsCallable(this); }
        public bool IsPrimitive() { return IsPrimitive(this); }


        public bool IsEnumerable()
        {
            // TODO try to implement although not necessary
            return false;
        }

        // resolve

        public Value ResolveRegister(ExecutionContext context)
        {
            if (Type != ValueType.Register || context == null)
                return this;

            var result = this;
            if (_number < context.RegisterCount && context.RegisterStored(_number))
            {
                var entry = context.GetRegister(_number);
                result = entry;
            }
            return result;
        }

        public Value ResolveConstant(ExecutionContext context)
        {
            if (Type != ValueType.Constant || context == null)
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

        public Value Resolve(ExecutionContext context) { return context == null ? this : ResolveReturn().ResolveConstant(context).ResolveRegister(context); }

        // from

        public static Value FromFunction(ASFunction func)
        {
            if (func == null)
                return Null();
            return new Value(ValueType.Object, o: func);
        }

        public static Value FromObject(ASObject obj)
        {
            if (obj != null && obj.IsFunction())
                return FromFunction((ASFunction) obj);
            else if (obj == null)
                return Null();
            return new Value(ValueType.Object, o: obj);
        }

        public static Value FromArray(Value[] array, VM vm)
        {
            return new Value(ValueType.Object, o: new ASArray(array, vm));
        }

        public static Value FromRegister(uint num)
        {
            return new Value(ValueType.Register, n: (int) num);
        }

        public static Value FromConstant(uint id)
        {
            return new Value(ValueType.Constant, n: (int) id);
        }

        public static Value ReturnValue(ExecutionContext actx)
        {
            return new Value(ValueType.Return, a: actx);
        }

        public static Value FromBoolean(bool cond)
        {
            return new Value(ValueType.Boolean, b: cond);
        }

        public static Value FromString(string str)
        {
            return new Value(ValueType.String, s: str);
        }

        public static Value FromInteger(int num)
        {
            return new Value(ValueType.Integer, n: num);
        }

        // TODO is it okay?
        public static Value FromUInteger(uint num)
        {
            if (num > 0x0FFFFFFF)
                return new Value(ValueType.Float, d: (double) num);
            else
                return new Value(ValueType.Integer, n: (int) num);
        }

        public static Value FromFloat(double num)
        {
            return new Value(ValueType.Float, d: num);
        }

        public static Value Null() { return new Value(ValueType.Null); }

        public static Value Undefined() { return new Value(ValueType.Undefined); }

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

        // conversion without AS


        public double ToFloat()
        {
            switch (Type)
            {
                case ValueType.Constant:
                case ValueType.Register:
                case ValueType.Integer:
                    return _number;
                case ValueType.Float:
                    return _decimal;
                case ValueType.Undefined:
                    return double.NaN;
                case ValueType.Null:
                    return +0;
                case ValueType.Boolean:
                    return _boolean ? 1 : +0;
                case ValueType.String:
                    return double.TryParse(_string, out var f) ? f : double.NaN;
                case ValueType.Object:
                    if (IsNull())
                        return +0;
                    else if (_object is ASString s)
                        return double.TryParse(s.ToString(), out var f2) ? f2 : double.NaN;
                    else
                        return double.NaN;
                default:
                    throw new NotImplementedException();
            }
        }

        // Follow ECMA specification 9.4: https://www.ecma-international.org/ecma-262/5.1/#sec-9.4
        // and optimized
        public int ToInteger()
        {
            switch (Type)
            {
                case ValueType.Constant:
                case ValueType.Register:
                case ValueType.Integer:
                    return _number;
                case ValueType.Float:
                    return Math.Sign(_decimal) * (int) Math.Abs(_decimal);
                case ValueType.Undefined:
                case ValueType.Null:
                    return 0;
                case ValueType.Boolean:
                    return _boolean ? 1 : 0;
                case ValueType.String:
                    return int.TryParse(_string, out var f) ? f : 0;
                case ValueType.Object:
                    if (IsNull())
                        return 0;
                    else if (_object is ASString s)
                        return int.TryParse(s.ToString(), out var f2) ? f2 : 0;
                    else
                        return 0;
                default:
                    throw new NotImplementedException();
            }
        }


        // conversion with AS
        // TODO \up

        // constant & register
        // Used by AptEditor to get actual id of constant / register
        public uint GetIDValue()
        {
            if (Type != ValueType.Constant && Type != ValueType.Register)
                throw new InvalidOperationException();

            return (uint) _number;
        }

        public T ToObject<T>() where T : ASObject
        {
            if (IsUndefined())
            {
                Logger.Error("Cannot create object from undefined!");
                return null;
            }
            else if (IsNull())
                return null;
            if (Type != ValueType.Object)
                throw new InvalidOperationException();

            return (T) _object;
        }

        public ASObject ToObject()
        {
            if (Type == ValueType.Undefined)
            {
                Logger.Error("Cannot create object from undefined!");
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

        // numbers

        internal Value ToNumber(ExecutionContext actx = null)
        {
            if (IsSpecialType())
            {
                var r = Resolve(actx);
                if (r.IsSpecialType()) return FromFloat(double.NaN);
                else return r.ToNumber(actx);
            }

            if (IsNumber())
                return this;
            else if (Type == ValueType.Boolean)
                return _boolean ? FromInteger(1) : FromInteger(0);
            else if (IsUndefined())
                return FromFloat(double.NaN);
            else if (IsNull())
                return FromInteger(0);
            else if (IsString())
                return
                    int.TryParse(_string, out var i) ? FromInteger(i) :
                    double.TryParse(_string, out var f) ? FromFloat(f) : FromFloat(double.NaN);
            else
            {
                var r = _object.DefaultValue(2, actx);
                if (r.Type == ValueType.Object && !IsNull(r)) return FromFloat(double.NaN);
                else return r.ToNumber(actx);
            }
        }

        

        public uint ToUInteger()
        {
            return (uint) ToInteger();
        }

        // ToReal() migrated to ToFloat()

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
                case ValueType.Null:
                    var = false;
                    break;
                case ValueType.Float:
                    var = (_decimal != 0);
                    break;
                case ValueType.Integer:
                    var = (_number != 0);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return var;
        }

        public ASFunction ToFunction()
        {
            if (Type == ValueType.Undefined)
            {
                Logger.Error("Undefined Function!");
                return null;
            }
            if (Type != ValueType.Object || _object is not ASFunction)
                throw new InvalidOperationException();

            return (ASFunction) _object;
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
                case ValueType.Integer:
                case ValueType.Constant:
                case ValueType.Register:
                    return _number.ToString();
                case ValueType.Float:
                    return _decimal.ToString();
                case ValueType.Undefined:
                    return "undefined"; // follows ECMA-262
                case ValueType.Null:
                    return "null";
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
                case ValueType.Float:
                    result = "number";
                    break;
                case ValueType.Object:
                    if (_object is MovieClip)
                        result = "movieclip";
                    else if (_object is ASFunction)
                        result = "function";
                    else
                        result = "object";
                    break;
                case ValueType.Undefined:
                    result = "undefined";
                    break;
                case ValueType.Null:
                    result = "null";
                    break;
                default:
                    throw new InvalidOperationException(Type.ToString());
            }
            return result;
        }

        // only used in debugging

        public string ToStringWithType(ExecutionContext ctx)
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

        // Follow ECMA specification 9.3: https://www.ecma-international.org/ecma-262/5.1/#sec-9.3\
        public TEnum ToEnum<TEnum>() where TEnum : struct
        {
            if (Type != ValueType.Integer)
                throw new InvalidOperationException();

            return EnumUtility.CastValueAsEnum<int, TEnum>(_number);
        }

        // TODO not comprehensive; ActionContext needed
        public Value ToPrimirive(int hint = 0, ExecutionContext actx = null)
        {
            switch (Type)
            {
                case ValueType.Undefined:
                case ValueType.Null:
                case ValueType.Boolean:
                case ValueType.Integer:
                case ValueType.Float:
                case ValueType.String:
                    return this;
                case ValueType.Object:
                    return _object.DefaultValue(hint, actx);
                default:
                    throw new NotImplementedException();
            }
            
        }

        // equality comparison

        /// <summary>
        /// used in dictionary, etc.
        /// do not use this in the VM
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(Value b)
        {
            return object.ReferenceEquals(this, b);
        }


        // used by ActionEquals
        public static bool NaiveEquals(Value a, Value b)
        {
            var fa = a.IsNumber() ? a.ToFloat() : 0;
            var fb = b.IsNumber() ? b.ToFloat() : 0;
            return fa == fb;
        }

        public static bool NumberEquals(Value a, Value b)
        {
            if (IsNumber(a) && IsNumber(b))
            {
                if (double.IsNaN(a._decimal) || double.IsNaN(b._decimal))
                    return false;
                else if (a.Type == ValueType.Integer && b.Type == ValueType.Integer)
                    return a._number == b._number;
                else
                {
                    var fa = a.ToFloat();
                    var fb = b.ToFloat();
                    return fa == fb || (Math.Abs(fa) == 0 && Math.Abs(fa) == Math.Abs(fb));
                }
            }
            else
                return false;
        }

        // used by ActionEquals2
        // The Abstract Equality Comparison Follows Section 11.9.3, ECMAScript Specification 3
        // https://262.ecma-international.org/5.1/#sec-11.9.3
        // https://www-archive.mozilla.org/js/language/E262-3.pdf
        public static bool AbstractEquals(Value x, Value y, ExecutionContext actx = null)
        {
            if ((IsNull(x) && IsNull(y)) ||
                (IsNull(x) && IsUndefined(y)) ||
                (IsNull(y) && IsUndefined(x)))
                return true;
            else if (IsNull(x) || IsNull(y))
                return false; // TODO check
            else if (x.Type == y.Type)
            {
                if (IsUndefined(x))
                    return true;
                else if (IsString(x))
                    return string.Equals(x.ToString(), y.ToString());
                else if (x.Type == ValueType.Boolean)
                    return x._boolean ^ y._boolean;
                else if (x.Type == ValueType.Object)
                    return x._object == y._object;
                else if (IsNumber(x))
                    return NumberEquals(x, y);
                else
                    return object.Equals(x, y); // TODO check
            }
            else if (IsNumber(x) && IsNumber(y))
                return NumberEquals(x, y);
            else
            {
                if (IsNumber(x) && IsString(y))
                    return NumberEquals(x, FromFloat(y.ToFloat()));
                else if (IsNumber(y) && IsString(x))
                    return NumberEquals(y, FromFloat(x.ToFloat()));
                else if (x.Type == ValueType.Boolean)
                    return AbstractEquals(FromFloat(x.ToFloat()), y);
                else if (y.Type == ValueType.Boolean)
                    return AbstractEquals(FromFloat(y.ToFloat()), x);
                else if ((IsNumber(x) || IsString(x)) && (y.Type == ValueType.Object && !IsNull(y)))
                    return AbstractEquals(x, y.ToPrimirive(2, actx));
                else if ((IsNumber(y) || IsString(y)) && (x.Type == ValueType.Object && !IsNull(x)))
                    return AbstractEquals(y, x.ToPrimirive(2, actx));
                else
                    return false;
            }
        }

        //TODO: Implement Strict Equality Comparison Algorithm
        public bool StrictEquals(Value b)
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
                default:
                    throw new NotImplementedException();
            }

            return result;
        }
    }
}
