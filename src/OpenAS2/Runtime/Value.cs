using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Base;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime
{
    public enum ValueType
    {
        Undefined,
        Null,

        Boolean,
        Integer,
        Float,

        String,
        Object,
    }

    public class Value
    {

        public readonly ValueType Type;

        private readonly string _string;
        private readonly bool _boolean;
        private readonly int _number; // 0 means +0
        private readonly double _decimal;
        private readonly ESObject? _object; // guarenteed to not ne null if type == object

        public readonly bool IsThrow = false;

        public string DisplayString { get; set; }

        protected Value(ValueType type,
            string? s = null,
            bool b = false,
            int n = 0,
            double d = 0,
            ESObject? o = null,
            bool isThrow = false)
        {
            Type = type;
            _string = s ?? string.Empty;
            _boolean = b;
            _number = n;
            _decimal = d;
            _object = o;

            IsThrow = isThrow;
            DisplayString = string.Empty;
        }

        // judgement


        public static bool IsUndefined(Value v) { return v != null && v.Type == ValueType.Undefined; }
        public static bool IsNull(Value v) { return v == null || (v.Type == ValueType.Null); }
        public static bool IsNumber(Value v) { return v != null && v.IsNumber(); }
        public static bool IsString(Value v) { return v != null && v.IsString(); }

        public bool IsUndefined() { return Type == ValueType.Undefined; }
        public bool IsNull() { return Type == ValueType.Null; }
        public bool IsNumber() { return Type == ValueType.Float || Type == ValueType.Integer; }
        public bool IsString() { return Type == ValueType.String || (Type == ValueType.Object && _object is ESString); }

        public static bool IsCallable(Value v) { return v != null && (v._object as ESFunction) != null; }
        public static bool IsPrimitive(Value v) { return v.Type != ValueType.Object; }

        public bool IsCallable() { return IsCallable(this); }
        public bool IsPrimitive() { return IsPrimitive(this); }


        public bool IsEnumerable()
        {
            // TODO try to implement although not necessary
            return false;
        }


        // from

        public static Value FromFunction(ESFunction? func)
        {
            if (func == null)
                return Null();
            return new Value(ValueType.Object, o: func);
        }

        public static Value FromObject(ESObject? obj)
        {
            if (obj != null && obj is ESFunction esf)
                return FromFunction(esf);
            else if (obj == null)
                return Null();
            return new Value(ValueType.Object, o: obj);
        }

        public static Value FromArray(IEnumerable<Value> array, VirtualMachine vm)
        {
            return new Value(ValueType.Object, o: new ESArray(array, vm));
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

        public static Value FromUInteger(uint num)
        {
            if (num > 0x0FFFFFFF)
                return new Value(ValueType.Float, d: (double)num);
            else
                return new Value(ValueType.Integer, n: (int)num);
        }

        public static Value FromFloat(double num)
        {
            return new Value(ValueType.Float, d: num);
        }

        public static Value Null() { return new Value(ValueType.Null); }

        public static Value Undefined() { return new Value(ValueType.Undefined); }

        public static Value FromRaw(RawValue s)
        {
            switch (s.Type)
            {
                case RawValueType.String:
                    return FromString(s.String);
                case RawValueType.Integer:
                    return FromInteger(s.Integer);
                case RawValueType.Float:
                    return FromFloat(s.Double);
                case RawValueType.Boolean:
                    return FromBoolean(s.Boolean);
                case RawValueType.Constant:
                case RawValueType.Register:
                default:
                    throw new InvalidOperationException("Well...This situation is really weird to be reached.");
            }
        }

        // error procession
        public static Value Throw(VirtualMachine? vm, ESError e)
        {
            if (vm == null)
                throw new InvalidOperationException("The throw statement requires a VM instance!");
            else
                return new(ValueType.Object, o: e, isThrow: true);
        }


        // conversion without AS
        public double ToFloat()
        {
            switch (Type)
            {
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
                    else if (_object is ESString s)
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
                case ValueType.Integer:
                    return _number;
                case ValueType.Float:
                    return Math.Sign(_decimal) * (int)Math.Abs(_decimal);
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
                    else if (_object is ESString s)
                        return int.TryParse(s.ToString(), out var f2) ? f2 : 0;
                    else
                        return 0;
                default:
                    throw new NotImplementedException();
            }
        }

        public ESObject ToObject()
        {
            if (Type != ValueType.Object)
                throw new InvalidOperationException();
            return _object!;
        }

        public ESObject ToObjectSafe()
        {
            if (Type != ValueType.Object)
                return null; // TODO
            return _object!;
        }

        public T ToObject<T>() where T : ESObject
        {
            if (Type != ValueType.Object)
                throw new InvalidOperationException();
            return (T)_object!;
        }

        public bool ToBoolean()
        {
            bool var;
            switch (Type)
            {
                case ValueType.String:
                    var = _string != null && _string.Length > 0;
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
                    var = (_decimal != 0) && (!double.IsNaN(_decimal));
                    break;
                case ValueType.Integer:
                    var = (_number != 0);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return var;
        }

        // conversion with AS

        public ESCallable.Result ToPrimitive(ExecutionContext context, int preferredType = 1)
        {
            // TODO prefetted type check
            if (IsPrimitive())
                return ESCallable.Return(this);
            else
                return _object!.IDefaultValue(context, preferredType);
        }

        public ESCallable.Result ToNumber(ExecutionContext ec)
        {

            Value ret = this;
            if (IsNumber())
                ret = this;
            else if (Type == ValueType.Boolean)
                ret = _boolean ? FromInteger(1) : FromInteger(0);
            else if (IsUndefined())
                ret = FromFloat(double.NaN);
            else if (IsNull())
                ret = FromInteger(0);
            else if (IsString())
                ret =
                    int.TryParse(_string, out var i) ? FromInteger(i) :
                    double.TryParse(_string, out var f) ? FromFloat(f) : FromFloat(double.NaN);
            else // object
            {
                var r = _object!.IDefaultValue(ec, 2);
                r.AddRecallCode(x => x.Value.ToNumber(ec));
                return r;
            }
            return ESCallable.Return(ret);
        }

        // tointeger & toint32 & touint32 & touint16: do it manually unless necessary

        public ESCallable.Result ToString(ExecutionContext ec)
        {
            if (Type != ValueType.Object)
                return ESCallable.Return(FromString(ToString()));
            var r = _object!.IDefaultValue(ec, 1);
            r.AddRecallCode(x => x.Value.ToString(ec));
            return r;
        }


        public ESCallable.Result ToObject(ExecutionContext ec)
        {
            // TODO std
            if (IsNull() || IsUndefined())
                return ESCallable.Throw(ec.ConstrutError("TypeError"));
            else if (Type == ValueType.Object)
                return ESCallable.Return(this);
            else if (Type == ValueType.Boolean)
                throw new NotImplementedException();
            else if (Type == ValueType.String)
                throw new NotImplementedException();
            else if (IsNumber())
                throw new NotImplementedException();
            else
                throw new InvalidOperationException();
        }

        // numbers

        public ESFunction ToFunction()
        {
            if (!IsCallable())
                throw new InvalidOperationException();
            return (ESFunction)_object!;
        }

        // Follow ECMA specification 9.8: https://www.ecma-international.org/ecma-262/5.1/#sec-9.8
        public override string ToString()
        {
            switch (Type)
            {
                case ValueType.String:
                    return _string;
                case ValueType.Boolean:
                    return _boolean ? "true" : "false";
                case ValueType.Integer:
                    return _number.ToString();
                case ValueType.Float:
                    return _decimal.ToString();
                case ValueType.Undefined:
                    return "undefined"; // follows ECMA-262
                case ValueType.Null:
                    return "null";
                case ValueType.Object:
                    return $"[object {_object!.IClass}]";
                default:
                    throw new InvalidOperationException(Type.ToString());
            }
        }

        public string GetStringType()
        {
            switch (Type)
            {
                case ValueType.String:
                    return "string";
                case ValueType.Boolean:
                    return "boolean";
                case ValueType.Integer:
                case ValueType.Float:
                    return "number";
                case ValueType.Object:
                    return _object!.ITypeOfResult;
                case ValueType.Undefined:
                    return "undefined";
                case ValueType.Null:
                    return "object";
                default:
                    throw new InvalidOperationException(Type.ToString());
            }
        }

        // only used in debugging

        public virtual string ToStringWithType(ExecutionContext ctx)
        {
            var ttype = Type.ToString().Substring(0, 3);
            string tstr = DisplayString;
            if (string.IsNullOrWhiteSpace(tstr))
            {
                tstr = ToString();
            }
            return $"({ttype}){tstr}";
        }

        public TEnum ToEnum<TEnum>(bool doThrow = false) where TEnum : struct
        {
            if (Type != ValueType.Integer)
                throw new InvalidOperationException();
            var ret = (TEnum)Enum.ToObject(typeof(TEnum), _number);
            if (Enum.IsDefined(typeof(TEnum), ret) && doThrow)
                throw new InvalidCastException();
            return ret;
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
        public static bool AbstractEquals(Value x, Value y, ExecutionContext? ec = null)
        {
            if ((IsNull(x) && IsNull(y)) ||
                (IsNull(x) && IsUndefined(y)) ||
                (IsNull(y) && IsUndefined(x)))
                return true;
            else if (IsNull(x) || IsNull(y))
                return false; // TODO check
            else if (x.Type == y.Type)
            {
                return StrictEquals(x, y, ec);
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
                    throw new InvalidOperationException();
                else if ((IsNumber(y) || IsString(y)) && (x.Type == ValueType.Object && !IsNull(x)))
                    throw new InvalidOperationException();
                else
                    return false;
            }
        }

        public static ESCallable.Result AbstractEqualsWrapped(ExecutionContext ec, Value x, Value y)
        {
            if ((IsNumber(x) || IsString(x)) && (y.Type == ValueType.Object))
                return AbstractEqualsWrapped(ec, x, y);
            else if ((IsNumber(y) || IsString(y)) && (x.Type == ValueType.Object))
            {
                var prim = x.ToPrimitive(ec, 2);
                prim.AddRecallCode(rc => ESCallable.Return(FromBoolean(AbstractEquals(rc.Value, y))));
                return prim;
            }
            else
                return ESCallable.Return(FromBoolean(AbstractEquals(x, y)));
        }

        //TODO: Implement Strict Equality Comparison Algorithm
        public static bool StrictEquals(Value x, Value y, ExecutionContext ec = null)
        {
            if (x.Type != y.Type)
                return false;
            else if (IsUndefined(x))
                return true;
            else if ((IsNull(x) || IsNull(y)) && !(IsNull(x) && IsNull(y)))
                return false;
            else if (IsString(x))
                return string.Equals(x.ToString(), y.ToString());
            else if (x.Type == ValueType.Boolean)
                return x._boolean ^ y._boolean;
            else if (x.Type == ValueType.Object)
                return x._object == y._object;
            else if (IsNumber(x))
                return NumberEquals(x, y);
            else
                return ESObject.EqualsES(x.ToObject()!, y.ToObject()!, ec);
        }

        public static bool SameValue(Value x, Value y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x.Type != y.Type)
                return false;
            if (x.IsNull() || x.IsUndefined())
                return true;
            if (x.IsNumber())
            {
                var xf = x.ToFloat();
                var yf = y.ToFloat();
                if (double.IsNaN(xf) && double.IsNaN(yf))
                    return true;
                else if (Math.Sign(xf) != Math.Sign(yf))
                    return false;
                else
                    return xf == yf;
            }
            if (x.Type == ValueType.Boolean)
                return x.ToBoolean() == y.ToBoolean();
            if (x.Type == ValueType.String)
                return x.ToString() == y.ToString();
            return ReferenceEquals(x._object, y._object);
        }

        // TODO
        // 11.8.5
        public static Value AbstractLess(Value x, Value y, ExecutionContext? ec = null)
        {
            var arg3 = x.ToFloat();
            var arg4 = y.ToFloat();
            Value res = null;

            if (double.IsNaN(arg3) || double.IsNaN(arg4))
                res = Value.Undefined();
            else
            {
                bool result2 = arg4 < arg3;
                res = Value.FromBoolean(result2);
            }
            return res;
        }

        public bool ToBoolean2()
        {
            switch (Type)
            {
                case ValueType.Boolean:
                    return _boolean;
                case ValueType.Integer:
                    return _number != 0;
                case ValueType.Float:
                    return _decimal != 0;
                case ValueType.String:
                    return double.TryParse(_string, out var res) ? res != 0 : false;
                default:
                    return false;
            }
            return false;
        }
    }
}
