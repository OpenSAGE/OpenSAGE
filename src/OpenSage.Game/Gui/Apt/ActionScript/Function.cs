using System;
using System.Collections.Generic;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Gui.Apt.ActionScript.Library;

namespace OpenSage.Gui.Apt.ActionScript
{
    [Flags]
    public enum FunctionPreloadFlags
    {
        PreloadExtern = 0x010000,   //this seems to be added by EA
        PreloadParent = 0x008000,
        PreloadRoot = 0x004000,

        SupressSuper = 0x002000,
        PreloadSuper = 0x001000,
        SupressArguments = 0x000800,
        PreloadArguments = 0x000400,
        SupressThis = 0x000200,
        PreloadThis = 0x000100,
        PreloadGlobal = 0x000001
    }

    public class FunctionArgument
    {
        public int Register;
        public string Parameter;
    }

    public abstract class Function: ObjectContext
    {

        public static new Dictionary<string, Func<VM, Property>> PropertiesDefined = new Dictionary<string, Func<VM, Property>>()
        {
            // methods
            ["apply"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (vm, tv, args) => { ((Function) tv).Apply(vm, tv, args); return null; }
                 , avm)), true, false, false),
            ["call"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (vm, tv, args) => { return ((Function) tv).Call(vm, tv, args); }
                 , avm)), true, false, false),
        };

        public static new Dictionary<string, Func<VM, Property>> StaticPropertiesDefined = new Dictionary<string, Func<VM, Property>>()
        {
            // ["prototype"] = (avm) => Property.D(Value.FromObject(avm.GetPrototype("Function")), true, false, false),
        };


        public Function() : this(null)
        {
        }

        public Function(VM vm): base(vm)
        {
            PrototypeInternal = vm is null ? null : vm.Prototypes["Function"];
            var prt = new ObjectContext(vm);
            prt.constructor = this;
            prototype = prt;
        }

        public abstract Value Invoke(ActionContext context, ObjectContext thisVar, Value[] args);

        public void Apply(ActionContext context, ObjectContext thisVar, Value[] args)
        {
            var thisVar_ = args.Length > 0 ? args[0] : Value.Undefined();
            var args_ = args.Length > 1 ? ((ASArray)args[1].ToObject()).GetValues() : new Value[0];
            Invoke(context, thisVar_.ToObject(), args_);
        }

        public Value Call(ActionContext context, ObjectContext thisVar, Value[] args)
        {
            var thisVar_ = Value.Undefined();
            var args_ = new Value[args.Length > 0 ? args.Length - 1 : 0];
            if (args.Length > 0) {
                thisVar_ = args[0];
                Array.Copy(args, 1, args_, 0, args_.Length);
            }
            return Invoke(context, thisVar_.ToObject(), args_);
        }

    }

    public class NativeFunction: Function
    {
        public Func<ActionContext, ObjectContext, Value[], Value> F { get; private set; }

        public NativeFunction(VM vm) : this(null, vm) { }

        public NativeFunction(Func<ActionContext, ObjectContext, Value[], Value> f, VM vm) : base(vm)
        {
            F = f;
        }

        public NativeFunction(ObjectContext pti) : base(null)
        {
            PrototypeInternal = pti;
        }

        public override Value Invoke (ActionContext context, ObjectContext thisVar, Value[] args) { return F(context, thisVar, args); }
    }

    public class DefinedFunction: Function
    {
        public DefinedFunction(VM vm): base(vm) { }

        public InstructionCollection Instructions { get; set; }
        public List<Value> Parameters { get; set; }
        public int NumberRegisters { get; set; }
        public List<Value> Constants { get; set; }
        public ActionContext DefinedContext { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }

        public override Value Invoke(ActionContext context, ObjectContext thisVar, Value[] args)
        {
            var vm = context.Apt.Avm;
            var acontext = GetContext(vm, args, thisVar);
            vm.PushContext(acontext);
            return Value.ReturnValue(acontext);
        }

        public ActionContext GetContext(VM vm, Value[] args, ObjectContext thisVar)
        {
            var context = vm.GetActionContext(DefinedContext, thisVar, NumberRegisters, Constants, Instructions);
            
            if (!IsNewVersion) // parameters in the old version are just stored as local variables
            {
                for (var i = 0; i < Parameters.Count; ++i)
                {
                    var name = Parameters[i].ToString();
                    bool provided = i < args.Length;
                    context.Params[name] = provided ? args[i] : Value.Undefined();
                }
            }
            else // parameters can be stored in both registers and local variables
            {
                for (var i = 0; i < Parameters.Count; i += 2)
                {
                    var reg = Parameters[i].ToInteger();
                    var name = Parameters[i + 1].ToString();
                    int argIndex = i >> 1;
                    bool provided = (argIndex) < args.Length;

                    if (reg != 0)
                    {
                        context.SetRegister(reg, provided ? args[argIndex] : Value.Undefined());
                    }
                    else
                    {
                        context.Params[name] = provided ? args[argIndex] : Value.Undefined();
                    }
                }
            }

            if (IsNewVersion)
            {
                context.Preload(Flags);
            }

            return context;
        }

    }

   
}
