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
        public static readonly Dictionary<string, Func<ExecutionContext, ESObject, Value[], Value>> BuiltinFunctions;
        public static Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> BuiltinVariables;
        public static DateTime InitTimeStamp { get; } = DateTime.Now;

        static Builtin()
        {
            // list of builtin objects and their corresponding constructors
            BuiltinClasses = new Dictionary<string, Type>()
            {
                ["Object"] = typeof(ESObject),
                ["Function"] = typeof(ESFunction),
                ["Array"] = typeof(ESArray),
                ["Color"] = typeof(ASColor),
                ["String"] = typeof(ESString),
                ["MovieClip"] = typeof(MovieClip),
                ["TextField"] = typeof(TextField),
            };

            // list of builtin functions
            BuiltinFunctions = new Dictionary<string, Func<ExecutionContext, ESObject, Value[], Value>>()
            {
                // Global constructors / functions
                ["Boolean"] = (actx, ctx, args) => Value.FromBoolean(args[0].ToBoolean()),
                ["Number"] = (actx, ctx, args) => args[0].ToNumber(),
                ["getTimer"] = (actx, ctx, args) => GetTimer(),
                ["clearInterval"] = ClearInterval,
                ["setInterval"] = SetInterval,
                ["ASSetPropFlags"] = (actx, ctx, args) => { ASSetPropFlags(
                    args[0].ToObject(),
                    args[1],
                    args.Length > 2 ? args[2].ToInteger() : 0,
                    args.Length > 3 ? args[3].ToInteger() : 0); return null; },
            };

            // list of builtin variables
            BuiltinVariables = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
            {
                // properties
                ["_root"] = (avm) => PropertyDescriptor.A(
                     (tv) => {
                         if (tv is not HostObject) return Value.Undefined();
                         return Value.FromObject(((HostObject) tv).Item.Context.Root.ScriptObject);
                     },
                     null, false, false),
                ["_global"] = (avm) => PropertyDescriptor.A(
                     (tv) => Value.FromObject(avm.GlobalObject),
                     null, false, false),
                ["extern"] = (avm) => PropertyDescriptor.A(
                     (tv) => Value.FromObject(avm.ExternObject),
                     null, false, false),

            };
        }

        
        public static Value GetTimer()
        {
            var result_ = DateTime.Now - InitTimeStamp;
            var result = Value.FromFloat(result_.TotalMilliseconds);
            return result;
        }

        public static Value SetInterval(ExecutionContext context, ESObject ctx, Value[] args)
        {
            var vm = context.Avm;
            var name = context.Pop().ToString();

            vm.CreateInterval(name, args[1].ToInteger(), args[0].ToFunction(), ctx, Array.Empty<Value>());
            ctx.IPut(name, Value.FromString(name));

            return null;
        }

        public static Value ClearInterval(ExecutionContext context, ESObject ctx, Value[] args)
        {
            var vm = context.Avm;
            var name = args[0].ToString();

            vm.ClearInterval(name);
            ctx.IDeleteValue(name);

            return null;
        }

        public static void ASSetPropFlags(ESObject obj, Value properties, int setFlags, int clearFlags)
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
        }

    }
}
