using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Library;
using ValueType = OpenAS2.Runtime.ValueType;

namespace OpenSage.Gui.Apt.Script
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

            BuiltinClasses = new Dictionary<string, Type>()
            {
                ["Color"] = typeof(ASColor),
                ["MovieClip"] = typeof(MovieClip),
                ["TextField"] = typeof(TextField),
            };

            BuiltinFunctions = new Dictionary<string, ESCallable.Func>()
            {
                // Temporary solution
                ["Boolean"] = (actx, ctx, args) => ESCallable.Return(Value.FromBoolean(args[0].ToBoolean())),
                ["Number"] = (actx, ctx, args) => args.First().ToNumber(actx), 
                
            };

            BuiltinVariables = new Dictionary<string, Func<PropertyDescriptor>>() {

            };

            /*
            // list of builtin variables
            BuiltinVariables = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
            {
                // properties
                ["_root"] = (avm) => PropertyDescriptor.A(
                     (tv) => {
                         if (tv is not StageObject) return Value.Undefined();
                         return Value.FromObject(((StageObject) tv).Item.Context.Root.ScriptObject);
                     },
                     null, false, false),
                ["_global"] = (avm) => PropertyDescriptor.A(
                     (tv) => Value.FromObject(avm.GlobalObject),
                     null, false, false),
                ["extern"] = (avm) => PropertyDescriptor.A(
                     (tv) => Value.FromObject(avm.ExternObject),
                     null, false, false),

            };
            */
        }


    }
}
