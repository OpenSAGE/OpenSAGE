using System;
using System.Collections.Generic;
using OpenAS2.Runtime.Opcodes;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime
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

    public abstract class ASFunction: ASObject
    {

        public static new Dictionary<string, Func<VirtualMachine, Property>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, Property>>()
        {
            // properties
            ["constructor"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     tv.__proto__ = actx.Avm.Prototypes["Function"];
                     return Value.FromObject(tv);
                 }, avm)), true, false, false),
            // methods
            ["apply"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (vm, tv, args) => { ((ASFunction) tv).Apply(vm, tv, args); return null; }
                 , avm)), true, false, false),
            ["call"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (vm, tv, args) => { return ((ASFunction) tv).Call(vm, tv, args); }
                 , avm)), true, false, false),
        };

        public static new Dictionary<string, Func<VirtualMachine, Property>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, Property>>()
        {
            // ["prototype"] = (avm) => Property.D(Value.FromObject(avm.GetPrototype("Function")), true, false, false),
        };


        public ASFunction() : this(null)
        {
        }

        public ASFunction(VirtualMachine vm): base(vm, "Function")
        {
            var prt = new ASObject(vm);
            prt.constructor = this;
            prototype = prt;
        }

        public abstract Value Invoke(ExecutionContext context, ASObject thisVar, Value[] args);

        public void Apply(ExecutionContext context, ASObject thisVar, Value[] args)
        {
            var thisVar_ = args.Length > 0 ? args[0] : Value.Undefined();
            var args_ = args.Length > 1 ? ((ASArray)args[1].ToObject()).GetValues() : new Value[0];
            Invoke(context, thisVar_.ToObject(), args_);
        }

        public Value Call(ExecutionContext context, ASObject thisVar, Value[] args)
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

    public class NativeFunction: ASFunction
    {
        public Func<ExecutionContext, ASObject, Value[], Value> F { get; private set; }

        public NativeFunction(VirtualMachine vm) : this(null, vm) { }

        public NativeFunction(Func<ExecutionContext, ASObject, Value[], Value> f, VirtualMachine vm) : base(vm)
        {
            F = f;
        }

        public NativeFunction(ASObject pti) : base(null)
        {
            PrototypeInternal = pti;
        }

        public override Value Invoke (ExecutionContext context, ASObject thisVar, Value[] args) { return F(context, thisVar, args); }
    }

    public class DefinedFunction: ASFunction
    {
        public DefinedFunction(VirtualMachine vm): base(vm) { }

        public InstructionCollection Instructions { get; set; }
        public List<Value> Parameters { get; set; }
        public int NumberRegisters { get; set; }
        public List<Value> Constants { get; set; }
        public ExecutionContext DefinedContext { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }

        public override Value Invoke(ExecutionContext context, ASObject thisVar, Value[] args)
        {
            var vm = context.Avm;
            var acontext = GetContext(vm, args, thisVar);
            vm.PushContext(acontext);
            return Value.ReturnValue(acontext);
        }

        public ExecutionContext GetContext(VirtualMachine vm, Value[] args, ASObject thisVar)
        {
            var context = vm.GetActionContext(DefinedContext, thisVar, NumberRegisters, Constants, Instructions);

            if (args == null)
                args = new Value[0];
            
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
                // the following codes follow the instructions in
                // page 114, swf file format spec v9 (ActionDefineFunction2 section)

                // first load parameters
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

                // then load variables
                // overwrite parameters if register is coincidently the same
                var a = Value.FromObject(new ASArray(args, vm));
                context.Preload(Flags, a);
            }

            return context;
        }

    }

   
}
