using System;
using System.Linq;
using System.Collections.Generic;
using OpenAS2.Base;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime
{
    using RawInstructionStorage = SortedList<int, RawInstruction>;
    public static class FunctionUtils
    {
        public static void DoDefineFunction(ExecutionContext context, IList<RawValue> Parameters)
        {
            var name = Parameters[0].String;
            var nParams = Parameters[1].Integer;
            var size = Parameters[2 + nParams].Integer;

            //create a list of parameters
            var paramList = Parameters
                .Skip(4)
                .Take(nParams * 2)
                .Select(x => Value.FromRaw(x));

            //get all the instructions
            var code = context.Stream.GetInstructions(size);

            var func = new DefinedFunction(context.Avm, context, paramList, context.Constants, code, false);

            var funcVal = Value.FromFunction(func);

            if (name.Length > 0)
                context.EnqueueResultCallback(context.This.IPut(context, name, funcVal));
            //anonymous function/lambda function
            else
                context.Push(funcVal);
        }

        public static void DoDefineFunction2(ExecutionContext context, IList<RawValue> Parameters)
        {
            var name = Parameters[0].String;
            var nParams = Parameters[1].Integer;
            var nRegisters = Parameters[2].Integer;
            FunctionPreloadFlags flags = (FunctionPreloadFlags) Parameters[3].Integer;
            var size = Parameters[4 + nParams * 2].Integer;

            //create a list of parameters
            var paramList = Parameters
                .Skip(4)
                .Take(nParams * 2)
                .Select(x => Value.FromRaw(x));

            //get all the instructions
            var code = context.Stream.GetInstructions(size);

            var func = new DefinedFunction(context.Avm, context, paramList, context.Constants, code, true, nRegisters, flags);
            

            var funcVal = Value.FromFunction(func);

            if (name.Length > 0)
                context.EnqueueResultCallback(context.This.IPut(context, name, funcVal));
            //anonymous function/lambda function
            else
                context.Push(funcVal);
        }

        public static IList<Value> GetArgumentsFromStack(ExecutionContext context)
        {
            var argCount = context.Pop().ToInteger();

            var args = new Value[argCount];
            for (int i = 0; i < argCount; ++i)
            {
                args[i] = context.Pop();
            }

            return args;
        }


        public static ESCallable.Result TryExecuteFunction(string funcName, IList<Value> args, ExecutionContext context, ESObject? thisVar = null)
        {
            return TryExecuteFunction(context.GetValueOnChain(funcName), args, context, thisVar);
        }
        public static ESCallable.Result TryExecuteFunction(Value funcVal, IList<Value> args, ExecutionContext context, ESObject? thisVar = null)
        {
            if (funcVal.IsCallable())
            {
                var func = funcVal.ToFunction();
                if (thisVar == null) thisVar = context.Global;
                return func.ICall(context, thisVar, args);
            }
            else
            {
                Logger.Warn($"Function val is wrong is wrong type: {funcVal}");
            }
            return new ESCallable.Result();
        }

        // conversions

        // special functions
    
        public static ESCallable.Result ReturnUndefined(ExecutionContext ec, ESObject tv, IList<Value>? args) { return ESCallable.Return(Value.Undefined()); }

        public static ESCallable.Result ThrowTypeError(ExecutionContext ec, ESObject tv, IList<Value>? args) {
            Value ret = ec.ConstrutError("TypeError", (args.Count > 0 ? args[0].ToString() : null) ?? string.Empty);
            return ESCallable.Throw(ret);
        }
    }


    public class ESFunction: ESObject
    {

        public override string ITypeOfResult => "function";

        // library

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            // properties

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
            
        };

        // definitions

        public virtual ESCallable.Func ICall { get; protected set; }
        public virtual ESCallable.Func IConstruct { get; protected set; }
        public ExecutionContext IScope { get; protected set; }
        public IList<string> IFormalParameters { get; protected set; }

        public bool Strict { get; protected set; }

        public bool Bind { get; protected set; }

        // base
        public ESFunction(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces,
            ESCallable.Func call,
            ESCallable.Func? construct = null,
            IEnumerable<string>? formalParameters = null) :
            base(classIndicator, extensible, prototype, interfaces)
        {
            ICall = call; // 6
            IConstruct = construct ?? IConstructDefault; // 7
            IFormalParameters = formalParameters == null ? new List<string>() : new List<string>(formalParameters); // 10~11
            IDefineOwnProperty(null, "length", PropertyDescriptor.D(Value.FromInteger(IFormalParameters.Count), false, false, false), false); // 15

        }

        // ecma262 v5.1 #13.2
        public ESFunction(
            VirtualMachine vm,
            ESCallable.Func call,
            ESCallable.Func? construct = null,
            ExecutionContext? definedScope = null,
            IEnumerable<string>? formalParameters = null,
            bool strict = false): base(vm, "Function", true)// 1~4
        {
            // 5

            ICall = call;// 6
            IConstruct = construct ?? IConstructDefault;// 7
            // 8 is not necessary
            IScope = definedScope ?? vm.GlobalContext; // TODO scope; 9

            // formal parameters
            IFormalParameters = formalParameters == null ? new List<string>() : new List<string>(formalParameters); // 10~11
            IDefineOwnProperty(null, "length", PropertyDescriptor.D(Value.FromInteger(IFormalParameters.Count), false, false, false), false); // 15

            var proto = new ESObject(vm); // TODO real new object; 16
            ConnectPrototype(proto); // 17~18

            if (strict)
                InitStrict(); // 19

            // 20
        }

        public void ConnectPrototype(ESObject proto, bool lockdown = false)
        {
            proto.IDefineOwnProperty(null, "constructor", PropertyDescriptor.D(Value.FromObject(this), true, false, true), false); // 17
            IDefineOwnProperty(null, "prototype", PropertyDescriptor.D(Value.FromObject(proto), true && !lockdown, false, false), false); // 18
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
        public static ESCallable.Result IConstructAndCall(ExecutionContext ec, ESObject tv, IList<Value> args)
        {
            var argCount = args.Count;
            var body = argCount == 0 ? null : args.Last();
            var formalArgs = argCount == 0 ? Enumerable.Empty<string>() : from v in args.SkipLast(1) select v.ToString();
            // TODO parse formal args
            // TODO parse body
            throw new NotImplementedException();
            ESCallable.Func parsedBody = null; 
            var strict = false;
            // TODO strict mode code
            return ESCallable.Return(Value.FromFunction(
                new ESFunction(ec.Avm, parsedBody, IConstructDefault, null, formalArgs, strict)
                ));
        }

        public static ESCallable.Result IConstructDefault(ExecutionContext ec, ESObject tv, IList<Value> args)
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
            return new ESCallable.Result(ec, _ =>
            {
                if (result.Type != ResultType.Executing)
                {
                    var rv = result.Value;
                    if (rv!.Type == ValueType.Object) // if null, also check the machine
                        return ESCallable.Return(rv);
                    return ESCallable.Return(Value.FromObject(obj));
                }
                else
                    throw new InvalidOperationException("should not happen, check the machine");
            });
            /*
            if (result.Type == ValueType.Object)
                return result;
            return Value.FromObject(obj);
            */
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
            var args_ = args.Count > 1 ? ((ESArray)args[1].ToObject()).GetValues() : new Value[0];
            ICall(context, thisVar_.ToObject(), args_);
        }

        public ESCallable.Result ICallNoThis(ExecutionContext context, ESObject thisVar, IList<Value> args)
        {
            // TODO WTF???
            var fakeThis = Value.Undefined();
            var args_ = new List<Value>(args.Skip(1));
            return ICall(context, fakeThis.ToObject(), args_);
        }

    }

    public class NativeFunction: ESFunction
    {
        public ESCallable.Func F { get; private set; }

        public NativeFunction(VirtualMachine vm) : this(vm, FunctionUtils.ReturnUndefined) { }

        public NativeFunction(VirtualMachine vm, ESCallable.Func? f, ExecutionContext? scope = null, IList<string>? formalParams = null) :
            base(vm, f ?? FunctionUtils.ReturnUndefined, IConstructDefault, definedScope: scope, formalParameters: formalParams)
        {

        }

    }

    public class DefinedFunction: ESFunction
    {
        public DefinedFunction(
            VirtualMachine vm,
            ExecutionContext scope,
            IEnumerable<Value> paramList,
            IList<Value> consts, 
            RawInstructionStorage code,
            bool isNewVersion = false,
            int nrReg = 4,
            FunctionPreloadFlags? flags = null
            ):
            base(vm, ICallDefault, IConstructDefault, scope)
        {
            Parameters = new List<Value>(paramList).AsReadOnly();
            Constants = consts;
            Instructions = code;
            IsNewVersion = isNewVersion;
            Flags = flags ?? 0;
            RegisterNumber = nrReg;
        }

        public RawInstructionStorage Instructions { get; set; }
        public IList<Value> Parameters { get; set; }
        public int RegisterNumber { get; set; }
        public IList<Value> Constants { get; set; }
        public FunctionPreloadFlags Flags { get; set; }
        public bool IsNewVersion { get; set; }

        public override ESCallable.Func ICall => ICallDefault;
        public static ESCallable.Result ICallDefault(ExecutionContext context, ESObject thisVar, IList<Value> args)
        {
            var vm = context.Avm;
            var thisFunc = (DefinedFunction) thisVar;
            var thisEC = thisFunc.GetContext(vm, args, thisVar);
            vm.PushContext(thisEC);
            return new(thisEC);
        }

        public ExecutionContext GetContext(VirtualMachine vm, IList<Value> args, ESObject thisVar)
        {
            var context = vm.CreateContext(IScope, thisVar, RegisterNumber, Constants, new(Instructions), null);

            if (args == null)
                args = new Value[0];
            
            if (!IsNewVersion) // parameters in the old version are just stored as local variables
            {
                for (var i = 0; i < Parameters.Count; ++i)
                {
                    var name = Parameters[i].ToString();
                    bool provided = i < args.Count;
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
                    bool provided = (argIndex) < args.Count;

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
                var a = Value.FromObject(new ESArray(args, vm));
                context.Preload(Flags, a);
            }

            return context;
        }

    }

   
}
