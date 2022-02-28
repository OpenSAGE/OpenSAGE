using System.Collections.Generic;
using System;
using System.Text;

namespace OpenAS2.Runtime.Library
{
    public class ESArray : ESObject
    {
        public static new readonly Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
            ["length"] = () => PropertyDescriptor.A(
               (ec, tv, args) => ESCallable.Return(Value.FromInteger(((ESArray)tv).GetLength())),
               null, false, false),
        };

        public static new readonly Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {
            ["toString"] = (ec, tv, args) =>
            {
                var ans = tv.IGet(ec, "join");
                ans.AddRecallCode(res =>
                {
                    if (!res.Value.IsCallable())
                        return ESCallable.Return(Value.FromString(tv.ToString()));
                    else
                        return res.Value.ToFunction().ICall(ec, tv, new Value[1] { Value.FromString(",") });
                });
                return ans;
            },
            ["join"] = (ec, tv, args) =>
            {
                if (!(tv is ESArray))
                    return ESCallable.Return(Value.FromString(""));
                var spr = ",";
                if (HasArgs(args) && !args![0].IsUndefined())
                    spr = args[0].ToString();
                return ((ESArray)tv).Join(ec, spr);
            },
        };

        public static new readonly Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
            // length is defined in construction
            // prototype is also defined in vm construction
        };

        public static new readonly Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {
            ["isArray"] = (ec, tv, args) =>
            {
                var ret = false;
                if (HasArgs(args))
                    ret = args![0].Type == ValueType.Object && args[0].ToObject().IClass == "Array";
                return ESCallable.Return(Value.FromBoolean(ret));
            }
        };

        public static readonly ESCallable.Func IConstructDefault = (ec, tv, args) =>
        {
            if (HasArgs(args) && args![0].IsNumber())
            {
                var num = args[0].ToNumber(ec);
                num.AddRecallCode(res =>
                {
                    var n = res.Value.ToFloat();
                    if (n < 0 || n % 1 != 0)
                        return ESCallable.Throw(ec.ConstrutError("RangeError"));
                    var arr = new ESArray(null, ec.Avm);
                    arr.ChangeLength(res.Value.ToInteger());
                    return ESCallable.Return(Value.FromObject(arr));
                });
                return num;
            }
            else
                return ESCallable.Return(Value.FromObject(new ESArray(args, ec.Avm)));
        };
        public static readonly ESCallable.Func ICallDefault = IConstructDefault;
        public static readonly IList<string> FormalParametersDefault = new List<string>() { "item" }.AsReadOnly();

        private List<Value> _values;
        public int GetLength() { return _values.Count; }
        public void ChangeLength(int len)
        {
            if (len < 0) throw new InvalidOperationException("Array length can not be less than 0.");
            else if (len == 0) _values.Clear();
            else if (len == _values.Count) return;
            else if (len < _values.Count)
            {
                while (len < _values.Count)
                    _values.RemoveAt(len);
            }
            else
            {
                while (len > _values.Count)
                    _values.Add(Value.Undefined());
            }
        }

        public ESArray(VirtualMachine vm) : base(vm, "Array")
        {
            _values = new() { Value.Undefined() };
        }

        public ESArray(IEnumerable<Value>? args, VirtualMachine vm) : base(vm, "Array")
        {
            if (args == null) _values = new List<Value>();
            else _values = new List<Value>(args);
        }

        public IList<Value> GetValues()
        {
            return new List<Value>(_values).AsReadOnly();
        }

        public Value GetValue(int index) { return _values[index]; }

        public override string ToString()
        {
            return $"[{string.Join(", ", _values)}]";
        }

        public ESCallable.Result Join(ExecutionContext ec, string separator = ", ")
        {
            if (_values.Count == 0)
                return ESCallable.Return(Value.FromString(""));
            else if (_values.Count == 0)
                return _values[0].ToString(ec);
            StringBuilder sb = new();

            for (int i = 0; i < _values.Count; ++i)
            {
                var ret = _values[i].ToString(ec);
                ret.AddRecallCode(res =>
                {
                    sb.Append(res.Value.ToString());
                    if (i < _values.Count - 1)
                        sb.Append(separator);
                    return null;
                });
                ec.EnqueueResultCallback(ret);
            }

            var res = ESCallable.Return(Value.FromString("If you see this it means something is problematic"));
            res.AddRecallCode(_ => ESCallable.Return(Value.FromString(sb.ToString())));
            return res;
        }
    }
}
