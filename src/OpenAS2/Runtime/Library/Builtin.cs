using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenAS2.Runtime;

namespace OpenAS2.Runtime.Library
{
    /// <summary>
    /// This class is meant to access builtin variables and return the corresponding value
    /// </summary>
    public static class Builtin
    {
        public static readonly Dictionary<string, Type> BuiltinClasses;
        public static readonly Dictionary<string, ESCallable.Func> BuiltinFunctions;
        public static Dictionary<string, Func<PropertyDescriptor>> BuiltinVariables;
        public static DateTime InitTimeStamp { get; } = DateTime.Now;

        static Builtin()
        {
            // list of builtin objects and their corresponding constructors
            BuiltinClasses = new Dictionary<string, Type>()
            {
                ["Object"] = typeof(ESObject),
                ["Function"] = typeof(ESFunction),
                ["Array"] = typeof(ESArray),
                // ["Color"] = typeof(ASColor),
                ["String"] = typeof(ESString),
                // ["MovieClip"] = typeof(MovieClip),
                // ["TextField"] = typeof(TextField),
            };

            // list of builtin functions
            BuiltinFunctions = new Dictionary<string, ESCallable.Func>()
            {
                // Global constructors / functions
                ["getTimer"] = GetTimer,
                ["clearInterval"] = ClearInterval,
                ["setInterval"] = SetInterval,
                ["ASSetPropFlags"] = (ec, ctx, args) =>
                {
                    if (!ESObject.HasArgs(args))
                        return ESCallable.Throw(ec.ConstrutError("TypeError"));
                    return ASSetPropFlags(
                    args[0].ToObject(),
                    args[1],
                    args.Count > 2 ? args[2].ToInteger() : 0,
                    args.Count > 3 ? args[3].ToInteger() : 0);
                },
            };

            // list of builtin variables
            BuiltinVariables = new Dictionary<string, Func<PropertyDescriptor>>()
            {
                ["undefined"] = () => PropertyDescriptor.D(Value.Undefined(), false, false, false),
                ["NaN"] = () => PropertyDescriptor.D(Value.FromFloat(double.NaN), false, false, false),
                ["Infinity"] = () => PropertyDescriptor.D(Value.FromFloat(double.PositiveInfinity), false, false, false),
            };
        }

        
        public static ESCallable.Result GetTimer(ExecutionContext context, ESObject ctx, IList<Value>? args)
        {
            var result_ = DateTime.Now - InitTimeStamp;
            var result = Value.FromFloat(result_.TotalMilliseconds);
            return ESCallable.Return(result);
        }

        public static ESCallable.Result SetInterval(ExecutionContext context, ESObject ctx, IList<Value>? args)
        {
            if (!ESObject.HasArgs(args))
                return ESCallable.Throw(context.ConstrutError("TypeError"));
            var vm = context.Avm;
            var name = context.Pop().ToString();

            vm.CreateInterval(name, args![1].ToInteger(), args[0].ToFunction(), ctx, Array.Empty<Value>());
            var intv = ctx.IPut(context, name, Value.FromString(name));

            return intv;
        }

        public static ESCallable.Result ClearInterval(ExecutionContext context, ESObject ctx, IList<Value>? args)
        {
            if (!ESObject.HasArgs(args))
                return ESCallable.Throw(context.ConstrutError("TypeError"));
            var vm = context.Avm;
            var name = args![0].ToString();

            vm.ClearInterval(name);
            var intv = ctx.IDeleteValue(context, name);

            return intv;
        }

        public static ESCallable.Result ASSetPropFlags(ESObject obj, Value properties, int setFlags, int clearFlags)
        {
            if (properties.Type == ValueType.String || (properties.Type == ValueType.Object && properties.ToObject() is ESString))
            {
                var _prop = properties.ToString();
                var _props = _prop.Split(", ");
                foreach (var p in _props)
                    obj.ASSetFlags(p, setFlags, clearFlags);
            }
            else if (properties.Type == ValueType.Object && properties.ToObject() is ESArray)
            {
                var _arr = properties.ToObject<ESArray>();
                var l = _arr.GetLength();
                for (int i = 0; i < l; ++i)
                    obj.ASSetFlags(_arr.GetValue(l).ToString(), setFlags, clearFlags);
            }
            else if ((properties.Type == ValueType.Object && properties.ToObject() == null) || properties == null) // null means all properties
                foreach (var p in obj.GetAllProperties())
                    obj.ASSetFlags(p, setFlags, clearFlags);
            else
                throw new InvalidOperationException($"Invalid argument: properties {properties}");
            return new ESCallable.Result();
        }

    }
}
