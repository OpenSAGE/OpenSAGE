using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Base;
using OpenAS2.Compilation;
using OpenAS2.Runtime.Library;
using OpenAS2.Runtime.Execution;
using OpenAS2.Runtime.Dom;

namespace OpenAS2.Runtime
{
    public sealed class ExecutionContext
    {

        public ESObject This { get; private set; }
        public ESObject Global { get; private set; }
        public ESObject Extern { get; private set; }
        public HostObject? Root { get; private set; }

        public Scope ReferredScope { get; private set; }

        public DomHandler? Dom => Avm == null ? null : Avm.Dom;
        public VirtualMachine Avm { get; private set; }

        public InstructionStream Stream { get; set; }
        public int RegisterCount { get; private set; }

        public ResultType Result { get; private set; }
        public bool Halted => Result == ResultType.Executing;
        public Value? ReturnValue { get; set; }

        public IList<ConstantEntry>? GlobalConstantPool { get; set; }
        public IList<Value> Constants { get; set; }

        public string DisplayName { get; set; } // only used for display purpose

        private Stack<Value> _stack;
        private LinkedList<ESCallable.Result> _callbacks;
        private Value[] _registers { get; set; }

        public ExecutionContext(
            VirtualMachine avm,
            ESObject globalVar,
            ESObject thisVar,
            ESObject externVar, 
            Scope scope,
            InstructionStream? stream,
            IList<ConstantEntry>? globalConstPool = null,
            IList<Value>? constPool = null,
            HostObject? rootVar = null,
            int numRegisters = 4,
            string? displayName = null
            )
        {
            // assignments
            Avm = avm;

            Global = globalVar;
            This = thisVar;
            Extern = externVar;
            Root = rootVar;

            ReferredScope = scope;
            Stream = stream ?? new(new(), true);
            RegisterCount = numRegisters;

            GlobalConstantPool = globalConstPool;
            Constants = constPool ?? new List<Value>().AsReadOnly();

            // initializations
            _stack = new();
            _callbacks = new();
            _registers = new Value[RegisterCount];
            Result = ResultType.Executing;

            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "[Unnamed]" : displayName;

        }

        // basics

        public void EnqueueResultCallback(ESCallable.Result code) { _callbacks.AddLast(code); }
        public ESCallable.Result DequeueResultCallback() { var f = _callbacks.First(); _callbacks.RemoveFirst(); return f; }
        public ESCallable.Result FirstResultCallback() { return _callbacks.First(); }
        public bool HasResultCallback() { return _callbacks.Count != 0; }

        public override string ToString()
        {
            return DisplayName == null ? "[ActionContext]" : $"[{DisplayName}]";
        }

        // constant operations

        public void ReformConstantPool(IList<RawValue> Parameters)
        {
            Constants = GlobalConstantPool == null ?
                new List<Value>().AsReadOnly() :
                CompilationUtils.CreateConstantPool(Parameters, GlobalConstantPool);
        }
        public Value ResolveConstant(int id)
        {
            var result = this;
            if (id < 0 || id >= Constants.Count)
            {
                throw new InvalidOperationException($"Constant number {id} is not appropriate! it should be 0~{Constants.Count - 1}.");
            }
            else
            {
                var entry = Constants[id];
                return entry;
            }
            // Logger.Error($"Constant resolves undefined at EC{DisplayName} ST{Stream} #{id}.");
            // return Value.Undefined();
        }

        // register operations

        public bool RegisterStored(int id) { return _registers[id] != null; }

        public Value GetRegister(int id)
        {
            if (id < 0 || id >= RegisterCount)
            {
                throw new InvalidOperationException($"Register number {id} is not appropriate! it should be 0~{RegisterCount - 1}.");
            }
            else if (RegisterStored(id))
            {
                return _registers[id];
            }
            return Value.Undefined();
        }

        public void SetRegister(int id, Value val)
        {
            if (id < 0 || id >= RegisterCount)
            {
                throw new InvalidOperationException($"Register number {id} is not appropriate! it should be 0~{RegisterCount - 1}.");
            }
            else
            {
                _registers[id] = val;
            }
        }

        public string DumpRegister()
        {
            var ans = $"Total {RegisterCount} Registers";
            for (int i = 0; i < RegisterCount; ++i)
            {
                if (_registers[i] != null) ans = ans + $"[{i}]{_registers[i].ToStringWithType()}";
            }
            return ans;
        }

        public string[] ListRegister()
        {
            var ans = new string[RegisterCount];
            var null_str = "unused";
            for (int i = 0; i < RegisterCount; ++i)
            {
                ans[i] = $"[{i}]{(_registers[i] == null ? null_str : _registers[i].ToStringWithType())}";
            }
            return ans;
        }

        public Value ResolveRegister(int id)
        {
            if (id < RegisterCount && RegisterStored(id))
            {
                var entry = GetRegister(id);
                return entry;
            }
            return Value.Undefined();
        }

        // stack operations

        public string DumpStack()
        {
            var stack_val = _stack.ToArray();
            var ans = string.Join("|", stack_val.Select(x => x.ToStringWithType()).ToArray());

            ans = string.Format("TOP|{0}|BOTTOM", ans);
            return ans;
        }

        public string[] ListStack()
        {
            var ans = new string[_stack.Count];
            for (int i = 0; i < _stack.Count; ++i)
            {
                ans[i] = $"[{i}]{_stack.ElementAt(i).ToStringWithType()}";
            }
            return ans;
        }

        public Value GetStackElement(int index) { return _stack.ElementAt(index); }

        public void Push(Value? v)
        {
            _stack.Push(v ?? Value.Undefined());
        }

        public void PushConstant(int id)
        {
            _stack.Push(Constants[id]);
        }

        public void Push(Value[] v)
        {
            if (v == null) return;
            foreach (var v_ in v)
                _stack.Push(v_);
        }

        public Value Peek()
        {
            var ret = _stack.Peek();
            return ret;
        }

        public Value Pop()
        {
            if (_stack.Count == 0)
            {
                Logger.Warn("[WARN] Trying to pop the stack while it is empty");
                return Value.Undefined();
            }
            var ret = _stack.Pop();
            return ret;
        }

        public Value[] Pop(uint count)
        {
            if (count < 1) return new Value[0];
            var ans = new Value[count];
            for (int i = 0; i < count; ++i)
                ans[i] = Pop();
            return ans;
        }

        public void SwapStack()
        {
            Stack<Value> snew = new();
            while (_stack.Count > 0)
                snew.Push(_stack.Pop());
            _stack = snew;
        }


        // special stack operations

        public void ExecUnaryOprOnStack(Func<Value, Value> opr)
        {
            _stack.Push(opr(_stack.Pop()));
        }

        public void ExecBinaryOprOnStack(Func<Value, Value, Value> opr)
        {
            _stack.Push(opr(_stack.Pop(), _stack.Pop()));
        }


        // local value operations

        /// <summary>
        /// Checks for special handled/global objects. After that checks for child objects of the
        /// current object
        /// </summary>
        /// <param name="name">the object name</param>
        /// <returns></returns>
        public ESCallable.Result GetValueOnChain(string name)
        {
            return ReferredScope.GetValueOnChain(this, name);
        }

        public ESCallable.Result SetValueOnLocal(string name, Value val)
        {
            return ReferredScope.SetValueOnLocal(this, name, val);
        }

        public bool DeleteValueOnChain(string name)
        {
            return ReferredScope.DeletePropertyOnChain(name);
        }


        /// <summary>
        /// Preload specified variables
        /// </summary>
        /// <param name="flags">the flags</param>
        public void Preload(FunctionPreloadFlags flags, Value args)
        {
            //preloaded variables start at register 1
            int reg = 1;

            //order is important
            if (flags.HasFlag(FunctionPreloadFlags.PreloadThis))
            {
                _registers[reg] = Value.FromObject(This);
                _registers[reg].DisplayString = "Preload This";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadArguments))
            {
                if (args == null)
                    throw new InvalidOperationException();
                _registers[reg] = args;
                _registers[reg].DisplayString = "Preload Arguments";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadSuper))
            {
                try
                {
                    _registers[reg] = Value.FromObject(This.IPrototype!.IPrototype);
                }
                catch
                {
                    _registers[reg] = Value.FromObject(null);
                }
                _registers[reg].DisplayString = "Preload Super";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadRoot))
            {
                _registers[reg] = Dom != null ? Value.FromObject(Root) : Value.Undefined();
                _registers[reg].DisplayString = "Preload Root";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadParent))
            {
                var parent = This as HostObject;
                if (parent != null) parent = parent.GetParent(this);
                _registers[reg] = parent != null ? Value.FromObject(parent) : Value.Undefined();
                _registers[reg].DisplayString = "Preload Parent";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadGlobal))
            {
                _registers[reg] = Value.FromObject(Avm.GlobalObject);
                _registers[reg].DisplayString = "Preload Global";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadExtern))
            {
                _registers[reg] = Value.FromObject(Extern);
                _registers[reg].DisplayString = "Preload Extern";
                ++reg;
            }
            if (!flags.HasFlag(FunctionPreloadFlags.SupressSuper))
            {
                try
                {
                    ReferredScope.SetValueOnLocal(this, "super", Value.FromObject(This.IPrototype!.IPrototype));
                }
                catch
                {
                    ReferredScope.SetValueOnLocal(this, "super", Value.Null());
                }
            }

            if (!flags.HasFlag(FunctionPreloadFlags.SupressArguments))
            {
                ReferredScope.SetValueOnLocal(this, "arguments", args);
            }

            if (!flags.HasFlag(FunctionPreloadFlags.SupressThis))
            {
                ReferredScope.SetValueOnLocal(this, "this", Value.FromObject(This));
            }
        }

        // executions

        public Value ConstrutError(string name = "Error", string message = "")
        {
            var err = Avm.ConstructError(name, message, this);
            Result = ResultType.Throw;
            ReturnValue = err;
            return err;
        }

        public void TryClearCallback()
        {
            while (HasResultCallback())
            {
                var rec = DequeueResultCallback();
                var res = rec.ExecuteRecallCode();
                if (res == null) // nothing to push to stack
                    continue;
                else if (res.Type == ResultType.Executing)
                { // new ECs are pushed, continue executing without cleaning
                    _callbacks.AddFirst(res);
                    break;
                }
                else if (res.Type == ResultType.Throw) // errors are thrown
                {
                    // TODO implement try-catch
                    Avm.ThrowError((ESError)res.Value.ToObject()!, res.Context, this);
                }
                else if (res.Type == ResultType.Return)
                    Push(res.Value);
                else
                    // throw new InvalidOperationException();
                    Logger.Warn("A callable result with type Normal is popped.");
            }
        }

        public RawInstruction? ExecuteOnceLocal(bool breakpoint = true)
        {
            if (Halted)
                throw new InvalidOperationException();
            else if (Stream.IsFinished())
            {
                Result = ResultType.Normal;
                ReturnValue = Pop();
            }
            else
            {
                var inst = Stream.GetCurrentInstruction();
                var doNotToNext = false;
                var handled = true;
                switch (inst.Type)
                {
                    // context variation or nothing
                    case InstructionType.End:
                        Result = ResultType.Normal;
                        ReturnValue = Pop();
                        break;
                    case InstructionType.Return:
                        Result = ResultType.Return;
                        ReturnValue = Pop();
                        break;
                    case InstructionType.Padding:
                        // do nothing
                        break;
                    case InstructionType.ConstantPool:
                        ReformConstantPool(inst.Parameters);
                        break;
                    case InstructionType.Throw:
                        Result = ResultType.Throw;
                        ReturnValue = Pop();
                        break;

                    // branch
                    // do nothing since stream handles all
                    case InstructionType.BranchAlways:
                        doNotToNext = true;
                        Stream.Branch(inst.Parameters[0].Integer);
                        break;
                    case InstructionType.BranchIfTrue:
                        if (doNotToNext = Pop().ToBoolean())
                            Stream.Branch(inst.Parameters[0].Integer);
                        break;
                    case InstructionType.EA_BranchIfFalse:
                        if (doNotToNext = !(Pop().ToBoolean()))
                            Stream.Branch(inst.Parameters[0].Integer);
                        break;

                    // code split
                    case InstructionType.DefineFunction:
                        FunctionUtils.DoDefineFunction(this, inst.Parameters);
                        break;
                    case InstructionType.DefineFunction2:
                        FunctionUtils.DoDefineFunction2(this, inst.Parameters);
                        break;
                    case InstructionType.Try:
                    case InstructionType.With:
                        handled = false;
                        break;

                    // dom related
                    case InstructionType.NextFrame:
                    case InstructionType.PrevFrame:
                    case InstructionType.Play:
                    case InstructionType.Stop:

                    case InstructionType.ToggleQuality:

                    case InstructionType.StopSounds:

                    case InstructionType.SetTarget:
                    case InstructionType.SetTarget2:
                    case InstructionType.TargetPath:

                    case InstructionType.CloneSprite:
                    case InstructionType.RemoveSprite:

                    case InstructionType.StartDragMovie:
                    case InstructionType.StopDragMovie:

                    case InstructionType.GotoLabel:
                    case InstructionType.GotoFrame:
                    case InstructionType.GotoFrame2:
                    case InstructionType.CallFrame:
                    case InstructionType.WaitFormFrame:
                    case InstructionType.WaitForFrameExpr:

                    case InstructionType.GetURL:
                    case InstructionType.GetURL2:

                    case InstructionType.GetTime:

                    case InstructionType.Trace:
                    case InstructionType.TraceStart:

                    case InstructionType.GetProperty:
                    case InstructionType.SetProperty:

                    case InstructionType.GetVariable:
                    case InstructionType.SetVariable:
                        // TODO
                        handled = Dom != null ? Dom.TryHandle(this, inst) : false;
                        break;

                    default:
                        handled = General.Execute(this, inst) && ObjectOriented.Execute(this, inst);

                        break;

                }
                if (!handled)
                    throw new NotImplementedException();

                if (!doNotToNext && !Halted)
                    Stream.ToNextInstruction();
                return inst;
            }
            return null;
        }

    }
}
