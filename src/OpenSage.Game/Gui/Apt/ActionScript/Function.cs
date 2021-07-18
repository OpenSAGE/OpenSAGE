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
        public static Function FunctionConstructor => _ffc;
        public static Function ObjectConstructor => _foc;

        public Function(): base()
        {
            __proto__ = FunctionPrototype;
            var prt = new ObjectContext();
            prt.constructor = this;
            prototype = prt;

            if (this is not SpecOp)
            {
                SetMember("apply", Value.FromObject(new SpecOp(Apply)));
                SetMember("call", Value.FromObject(new SpecOp(Call)));
            }
        }

        public Function(VM vm): base(vm)
        {

        }

        internal Function(bool JustUsedToCreateObjectPrototype) : base(JustUsedToCreateObjectPrototype)
        {
            __proto__ = FunctionPrototype;

            if (this is not SpecOp)
            {
                SetMember("apply", Value.FromObject(new SpecOp(Apply)));
                SetMember("call", Value.FromObject(new SpecOp(Call)));
            }
        }
        public abstract void Invoke(VM vm, ObjectContext thisVar, Value[] args);

        public void Apply(VM vm, ObjectContext thisVar, Value[] args)
        {
            var thisVar_ = args.Length > 0 ? args[0] : Value.Undefined();
            var args_ = args.Length > 1 ? ((ASArray)args[1].ToObject()).GetValues() : new Value[0];
            Invoke(vm, thisVar_.ToObject(), args_);
        }

        public void Call(VM vm, ObjectContext thisVar, Value[] args)
        {
            var thisVar_ = Value.Undefined();
            var args_ = new Value[args.Length > 0 ? args.Length - 1 : 0];
            if (args.Length > 0) {
                thisVar_ = args[0];
                Array.Copy(args, 1, args_, 0, args_.Length);
            }
            Invoke(vm, thisVar_.ToObject(), args_);
        }

    }

    public class SpecOp: Function
    {
        public Action<VM, ObjectContext, Value[]> F { get; private set; }
        public SpecOp(Action<VM, ObjectContext, Value[]> f): base()
        {
            F = f;
            SetMember("apply", Value.FromObject(this)); // Not sure if correct
            SetMember("call", Value.FromObject(this));
        }

        public SpecOp(Action<VM, ObjectContext, Value[]> f, VM vm) : base(vm)
        {
            F = f;
            SetMember("apply", Value.FromObject(this)); // Not sure if correct
            SetMember("call", Value.FromObject(this));
        }

        internal SpecOp(Action<VM, ObjectContext, Value[]> f, bool JustUsedToCreateObjectPrototype) : base(JustUsedToCreateObjectPrototype)
        {
            F = f;
            SetMember("apply", Value.FromObject(this)); // Not sure if correct
            SetMember("call", Value.FromObject(this));
        }

        public override void Invoke (VM vm, ObjectContext thisVar, Value[] args) { F(vm, thisVar, args); }
    }

    public class Function1: Function
    {

        public Function1(): base()
        {
        }

        public InstructionCollection Instructions { get; set; }
        public List<Value> Parameters { get; set; }
        public int NumberRegisters { get; set; }
        public List<Value> Constants { get; set; }
        public ActionContext DefinedContext { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }

        public override void Invoke(VM vm, ObjectContext thisVar, Value[] args)
        {
            var context = GetContext(vm, args, thisVar);
            vm.PushContext(context);
        }

        public ActionContext GetContext(VM vm, Value[] args, ObjectContext thisVar)
        {
            var context = vm.GetActionContext(DefinedContext, thisVar, NumberRegisters, Constants, Instructions);

            /*var localScope = new ObjectContext(thisVar.Item)
            {
                Constants = Constants,
                Variables = thisVar.Variables
            };

            var context = vm.GetActionContext(NumberRegisters, code, localScope, thisVar.Item.Character.Container.Constants.Entries);
            */
            //new ActionContext()
            //{
            //    Global = GlobalObject,
            //    Scope = localScope,
            //    Apt = scope.Item.Context,
            //    Stream = stream,
            //    Constants = scope.Item.Character.Container.Constants.Entries
            //};

            
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
