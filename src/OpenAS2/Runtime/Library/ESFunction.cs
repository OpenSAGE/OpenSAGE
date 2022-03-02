﻿using System;
using System.Linq;
using System.Collections.Generic;
using OpenAS2.Base;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime
{
    using RawInstructionStorage = SortedList<uint, RawInstruction>;
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

            var func = new DefinedFunction(context.Avm, context.ReferredScope, paramList, context.Constants, code, false);

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
            FunctionPreloadFlags flags = (FunctionPreloadFlags)Parameters[3].Integer;
            var size = Parameters[4 + nParams * 2].Integer;

            //create a list of parameters
            var paramList = Parameters
                .Skip(4)
                .Take(nParams * 2)
                .Select(x => Value.FromRaw(x));

            //get all the instructions
            var code = context.Stream.GetInstructions(size);

            var func = new DefinedFunction(context.Avm, context.ReferredScope, paramList, context.Constants, code, true, nRegisters, flags);


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
            var f = context.GetValueOnChain(funcName);
            f.AddRecallCode(res => TryExecuteFunction(res.Value, args, context, thisVar));
            return f;
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

        public static ESCallable.Result ThrowTypeError(ExecutionContext ec, ESObject tv, IList<Value>? args)
        {
            Value ret = ec.ConstrutError("TypeError", (ESObject.HasArgs(args) ? args![0].ToString() : null) ?? string.Empty);
            return ESCallable.Throw(ret);
        }
    }


    public class ESFunction : ESObject
    {

        public override string ITypeOfResult => "function";

        // library

        public static new Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {

        };

        public static new Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {
            ["apply"] = Apply,
            ["call"] = Call,
            ["bind"] = Bind,
        };

        public static new Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
            // length is defined in construction
            // prototype is also defined in vm construction
        };

        public static new Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };

        // definitions

        public virtual ESCallable.Func ICall { get; protected set; }
        public virtual ESCallable.Func IConstruct { get; protected set; }
        public Scope IScope { get; protected set; }
        public IList<string> IFormalParameters { get; protected set; }

        public bool Strict { get; protected set; }

        public bool IsBoundFunction { get; protected set; }
        private ESFunction? _bindTarget = null;

        // base
        public ESFunction(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces,
            ESCallable.Func call,
            ESCallable.Func? construct,
            Scope definedScope,
            IEnumerable<string>? formalParameters = null) :
            base(classIndicator, extensible, prototype, interfaces)
        {
            ICall = call; // 6
            IConstruct = construct ?? IConstructDefault; // 7
            // !8
            IScope = definedScope; // 9
            IFormalParameters = formalParameters == null ? new List<string>() : new List<string>(formalParameters); // 10~11
            IDefineOwnProperty(null, "length", PropertyDescriptor.D(Value.FromInteger(IFormalParameters.Count), false, false, false), false); // 15

        }

        // ecma262 v5.1 #13.2
        public ESFunction(
            VirtualMachine vm,
            ESCallable.Func call,
            ESCallable.Func? construct = null,
            Scope? definedScope = null,
            IEnumerable<string>? formalParameters = null,
            ESObject? proto = null,
            bool strict = false) : base(vm, "Function", extensible: true)// 1~4
        {
            // 5

            ICall = call;// 6
            IConstruct = construct ?? IConstructDefault;// 7
            // 8 is not necessary
            IScope = definedScope ?? vm.GlobalScope; // 9

            // formal parameters
            IFormalParameters = formalParameters == null ? new List<string>() : new List<string>(formalParameters); // 10~11
            IDefineOwnProperty(null, "length", PropertyDescriptor.D(Value.FromInteger(IFormalParameters.Count), false, false, false), false); // 15

            proto = proto ?? new ESObject(vm); // TODO real new object; 16
            ConnectPrototype(proto); // 17~18

            Strict = strict; // 19a
            if (strict)
                InitStrict(vm); // 19

            // 20
        }

        public void ConnectPrototype(ESObject proto, bool lockdown = false)
        {
            proto.IDefineOwnProperty(null, "constructor", PropertyDescriptor.D(Value.FromObject(this), true, false, true), false); // 17
            IDefineOwnProperty(null, "prototype", PropertyDescriptor.D(Value.FromObject(proto), true && !lockdown, false, false), false); // 18
        }

        public void InitStrict(VirtualMachine vm)
        {
            ESCallable.Func thrower = FunctionUtils.ThrowTypeError;
            IDefineOwnProperty(vm.GlobalContext, "caller", PropertyDescriptor.A(thrower, thrower, false, false), false); // 19b
            IDefineOwnProperty(vm.GlobalContext, "arguments", PropertyDescriptor.A(thrower, thrower, false, false), false); // 19c
        }

        // internal

        public override ESCallable.Result IGet(ExecutionContext ec, string name)
        {
            var v = base.IGet(ec, name);
            v.AddRecallCode(x =>
            {
                var v = x.Value;
                if (!IsBoundFunction && name == "caller" && v.Type == ValueType.Object && v.ToObject() is ESFunction vf && vf.Strict)
                    return ESCallable.Return(Value.Undefined());
                return x;
            });
            return v;
        }

        // both [[Call]] and [[Construct]]
        public static ESCallable.Result IConstructAndCall(ExecutionContext ec, ESObject tv, IList<Value>? args)
        {
            throw new NotImplementedException("compiler is not usable yet");
            /*
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
                new ESFunction(ec.Avm, parsedBody, IConstructDefault, null, formalArgs, null, strict)
                ));
            */
        }

        public static ESCallable.Result IConstructDefault(ExecutionContext ec, ESObject tv, IList<Value>? args)
        {
            if (!(tv is ESFunction))
                return ESCallable.Throw(ec.ConstrutError("TypeError"));
            var tvf = (ESFunction)tv;
            var protov = tvf.IGet(ec, "prototype");
            protov.AddRecallCode(res =>
            {
                ESObject objProto;
                if (res.Value.Type == ValueType.Object)
                    objProto = res.Value.ToObject();
                else
                    ec.Avm.Prototypes.TryGetValue("Object", out objProto!);

                var obj = new ESObject("Object", true, objProto, null);
                var result = tvf.ICall(ec, obj, args);
                return new ESCallable.Result(ec, _ =>
                {
                    var rv = result.Value;
                    if (rv!.Type == ValueType.Object) // if null, also check the machine
                        return ESCallable.Return(rv);
                    return ESCallable.Return(Value.FromObject(obj));
                });
            });
            return protov;
        }

        public ESCallable.Result IHasInstance(ExecutionContext ec, Value v)
        {
            if (IsBoundFunction)
                return _bindTarget!.IHasInstance(ec, v);
            var obj = v.ToObject();
            if (obj != null || v.Type != ValueType.Object)
            {
                var prot2 = IGet(ec, "prototype");
                prot2.AddRecallCode(res =>
                {
                    var prot = res.Value;
                    if (prot.Type != ValueType.Object)
                    {
                        return ESCallable.Throw(ec.ConstrutError("TypeError"));
                    }
                    var ret = false;
                    var pobj = prot.ToObject();
                    while (obj != null)
                    {
                        obj = obj.IPrototype;
                        if (obj == pobj)
                        {
                            ret = true;
                            break;
                        }
                    }
                    return ESCallable.Return(Value.FromBoolean(ret));
                });
                return prot2;
            }
            return ESCallable.Throw(ec.ConstrutError("TypeError"));
        }


        public static ESCallable.Result Apply(ExecutionContext context, ESObject thisVar, IList<Value>? args)
        {
            if (!(thisVar is ESFunction))
                return ESCallable.Throw(context.ConstrutError("TypeError"));
            var target = (ESFunction)thisVar;
            var fakeThis = HasArgs(args) ? args![0] : Value.Undefined();
            var fakeArgs = args!.Count > 1 ? ((ESArray)args[1].ToObject()).GetValues() : new List<Value>().AsReadOnly();
            return target.ICall(context, fakeThis.ToObjectSafe(), fakeArgs);
        }

        public static ESCallable.Result Call(ExecutionContext context, ESObject thisVar, IList<Value>? args)
        {
            if (!(thisVar is ESFunction))
                return ESCallable.Throw(context.ConstrutError("TypeError"));
            var target = (ESFunction)thisVar;
            var fakeThis = HasArgs(args) ? args![0] : Value.Undefined();
            var fakeArgs = new List<Value>(args != null ? args.Skip(1) : new Value[0]).AsReadOnly();
            return target.ICall(context, fakeThis.ToObjectSafe(), fakeArgs);
        }

        public static ESCallable.Result Bind(ExecutionContext context, ESObject thisVar, IList<Value>? args)
        {
            if (!(thisVar is ESFunction))
                return ESCallable.Throw(context.ConstrutError("TypeError"));
            var target = (ESFunction)thisVar;
            var fakeThis = (HasArgs(args) ? args![0] : Value.Undefined()).ToObjectSafe();
            var fakeArgs = new List<Value>(args != null ? args.Skip(1) : new Value[0]).AsReadOnly();
            ESCallable.Func fakeICall = (ec, _, args) => target.ICall(ec, fakeThis, args == null ? fakeArgs : fakeArgs.Union(args).ToList().AsReadOnly());
            ESCallable.Func fakeIConstruct = (ec, _, args) =>
            {
                if (target.IConstruct == null)
                    return ESCallable.Throw(ec.ConstrutError("TypeError"));
                return target.IConstruct(ec, fakeThis, args == null ? fakeArgs : fakeArgs.Union(args).ToList().AsReadOnly());
            };
            ESFunction f = new NativeFunction(context.Avm, fakeICall, construct: fakeIConstruct, scope: context.ReferredScope, formalParams: target.IFormalParameters.Skip(fakeArgs.Count));
            f.IsBoundFunction = true;
            f._bindTarget = target;
            f.InitStrict(context.Avm);
            return ESCallable.Return(Value.FromFunction(f));
        }

    }

    public class NativeFunction : ESFunction
    {
        public NativeFunction(VirtualMachine vm) : this(vm, FunctionUtils.ReturnUndefined) { }

        public NativeFunction(VirtualMachine vm,
            ESCallable.Func? f,
            ESCallable.Func? construct = null,
            Scope? scope = null,
            IEnumerable<string>? formalParams = null) :
            base(vm, f ?? FunctionUtils.ReturnUndefined, construct ?? IConstructDefault, definedScope: scope, formalParameters: formalParams)
        {

        }

    }

    public class DefinedFunction : ESFunction
    {
        public DefinedFunction(
            VirtualMachine vm,
            Scope scope,
            IEnumerable<Value> paramList,
            IList<Value> consts,
            RawInstructionStorage code,
            bool isNewVersion = false,
            int nrReg = 4,
            FunctionPreloadFlags? flags = null
            ) :
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
        public static ESCallable.Result ICallDefault(ExecutionContext context, ESObject thisVar, IList<Value>? args)
        {
            var vm = context.Avm;
            var thisFunc = (DefinedFunction)thisVar;
            var thisEC = thisFunc.GetContext(context, args ?? new Value[0], thisVar);
            vm.PushContext(thisEC);
            return new(thisEC);
        }

        public ExecutionContext GetContext(ExecutionContext parent, IList<Value> args, ESObject thisVar)
        {
            var context = parent.Avm.CreateContext(parent, IScope, thisVar, RegisterNumber, Constants, new(Instructions), null);

            if (args == null)
                args = new Value[0];

            if (!IsNewVersion) // parameters in the old version are just stored as local variables
            {
                var s = context.ReferredScope;
                for (var i = 0; i < Parameters.Count; ++i)
                {
                    var name = Parameters[i].ToString();
                    bool provided = i < args.Count;
                    var p = provided ? args[i] : Value.Undefined();
                    s.PutPropertyOnLocal(name, PropertyDescriptor.D(p, true, false, true));
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
                        var p = provided ? args[argIndex] : Value.Undefined();
                        context.ReferredScope.PutPropertyOnLocal(name, PropertyDescriptor.D(p, true, false, true));
                    }
                }

                // then load variables
                // overwrite parameters if register is coincidently the same
                var a = Value.FromObject(new ESArray(args, parent.Avm));
                context.Preload(Flags, a);
            }

            return context;
        }

    }


}