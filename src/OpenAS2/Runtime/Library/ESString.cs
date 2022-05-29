using System.Collections.Generic;
using System;

namespace OpenAS2.Runtime.Library
{
    public class ESString : ESObject
    {
        public static new readonly Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
            ["length"] = () => PropertyDescriptor.A(
               (ec, tv, args) => ESCallable.Return(Value.FromInteger(((ESString)tv).GetLength())),
               null, false, false),
        };

        public static new readonly Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {
            ["substr"] =
                 (vm, tv, args) => ((ESString)tv).substr(args)
                 ,
        };

        public static new readonly Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
            // length is defined in construction
            // prototype is also defined in vm construction
        };

        public static new readonly Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };

        public static readonly ESCallable.Func IConstructDefault = (ec, tv, args) =>
        {
            if (HasArgs(args) && args![0].IsNumber())
            {
                var num = args[0].ToString(ec);
                num.AddRecallCode(res =>
                {
                    var arr = new ESString(res.Value, ec.Avm);
                    return ESCallable.Return(Value.FromObject(arr));
                });
                return num;
            }
            else
                return ESCallable.Return(Value.FromObject(new ESString(null, ec.Avm)));
        };
        public static readonly ESCallable.Func ICallDefault = (ec, tv, args) =>
            HasArgs(args) ?
            ESCallable.Return(Value.FromString(string.Empty)) :
            args![0].ToString(ec);
        public static readonly IList<string> FormalParametersDefault = new List<string>() { "str" }.AsReadOnly();

        private string _value;

        public ESString(VirtualMachine vm) : this(null, vm) { }
        public ESString(Value? value, VirtualMachine vm) : base(vm, "String")
        {
            if (value == null) _value = "";
            else _value = value.ToString();
        }
        public int GetLength() { return _value.Length; }

        public ESCallable.Result substr(IList<Value>? args)
        {
            var ret = _value;
            if (!HasArgs(args))
            {
                // do nothing
            }
            if (args!.Count == 1)
            {
                ret = _value.Substring(args[0].ToInteger());
            }
            else
            {
                ret = _value.Substring(args[0].ToInteger(), args[1].ToInteger());
            }
            return ESCallable.Return(Value.FromString(ret));
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
