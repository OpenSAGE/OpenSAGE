using System;
using System.Linq;
using System.Collections.Generic;
using OpenAS2.Base;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime
{
    
    public static class FunctionUtils
    {
        public static Value ReturnUndefined(ExecutionContext ec, ESObject tv, IList<Value> args) { return Value.Undefined(); }

        public static Value ThrowTypeError(ExecutionContext ec, ESObject tv, IList<Value> args) {
            ec.Avm.ThrowError(new ESError());
            return Value.Undefined();
        }
    }


    public class ESFunction: ESObject
    {
        

        // library

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            // properties
            ["constructor"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (ec, tv, args) => {
                     tv.__proto__ = ec.Avm.Prototypes["Function"];
                     return Value.FromObject(tv);
                 }, avm)), true, false, false),
            // methods
            ["apply"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (ec, tv, args) => { ((ESFunction) tv).Apply(ec, tv, args); return null; }
                 , avm)), true, false, false),
            ["call"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (ec, tv, args) => { return ((ESFunction) tv).ICallNoThis(ec, tv, args); }
                 , avm)), true, false, false),
        };

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            // ["prototype"] = (avm) => Property.D(Value.FromObject(avm.GetPrototype("Function")), true, false, false),
        };

        // definitions

        public ESCallable ICall { get; protected set; }
        public ESCallable IConstruct { get; protected set; }
        public ExecutionContext IScope { get; protected set; }
        public InstructionCollection ICode { get; protected set; }
        public IList<string> IFormalParameters { get; protected set; }

        public bool Strict { get; protected set; }

        public bool Bind { get; protected set; }

        // base
        public ESFunction(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces,
            ESCallable call,
            ESCallable? construct = null,
            IList<string>? formalParameters = null) :
            base(classIndicator, extensible, prototype, interfaces)
        {
            ICall = call; // 6
            IConstruct = construct ?? IConstructDefault; // 7
            IFormalParameters = formalParameters == null ? new List<string>() : new List<string>(formalParameters); // 10~11
            IDefineOwnProperty("length", PropertyDescriptor.D(Value.FromInteger(IFormalParameters.Count), false, false, false), false); // 15

        }

        // ecma262 v5.1 #13.2
        public ESFunction(
            VirtualMachine vm,
            ESCallable call,
            ESCallable? construct = null, 
            InstructionCollection code,
            IEnumerable<string>? formalParameters = null,
            bool strict = false): base(vm, "Function", true)// 1~4
        {
            // 5

            ICall = call;// 6
            IConstruct = construct ?? IConstructDefault;// 7
            ICode = code; // 8; TODO not necessary
            // TODO scope; 9

            // formal parameters
            IFormalParameters = formalParameters == null ? new List<string>() : new List<string>(formalParameters); // 10~11
            IDefineOwnProperty("length", PropertyDescriptor.D(Value.FromInteger(IFormalParameters.Count), false, false, false), false); // 15

            var proto = new ESObject(vm); // TODO real new object; 16
            ConnectPrototype(proto); // 17~18

            if (strict)
                InitStrict(); // 19

            // 20
        }

        public void ConnectPrototype(ESObject proto, bool lockdown = false)
        {
            proto.IDefineOwnProperty("constructor", PropertyDescriptor.D(Value.FromObject(this), true, false, true), false); // 17
            IDefineOwnProperty("prototype", PropertyDescriptor.D(Value.FromObject(proto), true && !lockdown, false, false), false); // 18
        }

        public void InitStrict()
        {
            Strict = true;
            throw new NotImplementedException("thrower"); // 19a
                                                          // IDefineOwnProperty("caller", PropertyDescriptor.A(thrower, thrower, false, false), false); // 19b
                                                          // IDefineOwnProperty("arguments", PropertyDescriptor.A(thrower, thrower, false, false), false); // 19c
        }

        // internal

        public override Value IGet(string name)
        {
            var v = base.IGet(name);
            if (!Bind && name == "caller" && v.Type == ValueType.Object && v.ToObject() is ESFunction vf && vf.Strict)
            {
                // TODO throw type error
                return Value.Undefined();
            }
            return v;
        }

        // both [[Call]] and [[Construct]]
        public static Value IConstructAndCall(ExecutionContext ec, ESObject tv, IList<Value> args)
        {
            var argCount = args.Count;
            var body = argCount == 0 ? null : args.Last();
            var formalArgs = argCount == 0 ? Enumerable.Empty<string>() : from v in args.SkipLast(1) select v.ToString();
            // TODO parse formal args
            // TODO parse body
            var strict = false;
            // TODO strict mode code
            return Value.FromFunction(new ESFunction(ec.Avm, ESObject.ICall, IConstruct, null, formalArgs, strict));
        }

        public static Value IConstructDefault(ExecutionContext ec, ESObject tv, IList<Value> args)
        {
            var tvf = (ESFunction) tv;
            var protov = tvf.IGet("prototype");
            ESObject objProto;
            if (protov.Type == ValueType.Object)
            {
                var proto = protov.ToFunction();
                objProto = proto;
            }
            else
                ec.Avm.Prototypes.TryGetValue("Object", out objProto!);
            var obj = new ESObject("Object", true, objProto, null);
            var result = ((ESFunction) tv).ICall(ec, obj, args);
            if (result.Type == ValueType.Object)
                return result;
            return Value.FromObject(obj);
        }

        public bool IHasInstance(Value v)
        {
            var obj = v.ToObject();
            if (obj != null || v.Type != ValueType.Object)
            {
                var prot = IGet("prototype");
                if (prot.Type != ValueType.Object)
                {
                    // TODO throw typeerror
                }
                var pobj = prot.ToObject();
                while (obj != null)
                {
                    obj = obj.IPrototype;
                    if (obj == pobj)
                        return true;
                }
            }
            return false;
        }
        

        public void Apply(ExecutionContext context, ESObject thisVar, IList<Value> args)
        {
            var thisVar_ = args.Count > 0 ? args[0] : Value.Undefined();
            var args_ = args.Count > 1 ? ((ASArray)args[1].ToObject()).GetValues() : new Value[0];
            ICall(context, thisVar_.ToObject(), args_);
        }

        public Value ICallNoThis(ExecutionContext context, ESObject thisVar, IList<Value> args)
        {
            var thisVar_ = Value.Undefined();
            var args_ = new List<Value>(args.Skip(1));
            return ICall(context, thisVar_.ToObject(), args_);
        }

    }

    public class NativeFunction: ESFunction
    {
        public ESCallable F { get; private set; }

        public NativeFunction(VirtualMachine vm) : this(null, vm) { }

        public NativeFunction(ESCallable f, VirtualMachine vm) : base(vm)
        {
            F = f;
        }

        public NativeFunction(ESObject pti) : base(null)
        {
            IPrototype = pti;
        }

        public override Value ICall (ExecutionContext context, ESObject thisVar, IList<Value> args) { return F(context, thisVar, args); }
    }

    public class DefinedFunction: ESFunction
    {
        public DefinedFunction(VirtualMachine vm): base(vm) { }

        public InstructionCollection Instructions { get; set; }
        public List<Value> Parameters { get; set; }
        public int NumberRegisters { get; set; }
        public List<Value> Constants { get; set; }
        public ExecutionContext DefinedContext { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }

        public override Value ICall(ExecutionContext context, ESObject thisVar, IList<Value> args)
        {
            var vm = context.Avm;
            var acontext = GetContext(vm, args, thisVar);
            vm.PushContext(acontext);
            return Value.ReturnValue(acontext);
        }

        public ExecutionContext GetContext(VirtualMachine vm, Value[] args, ESObject thisVar)
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
